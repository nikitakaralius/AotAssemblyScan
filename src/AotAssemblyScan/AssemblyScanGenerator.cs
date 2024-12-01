using System.Collections.Immutable;
using System.Text;
using AotAssemblyScan.TypeFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AotAssemblyScan;

[Generator]
public sealed class AssemblyScanGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var partialAssemblyScanMethodsProvider = context
           .SyntaxProvider
           .ForAttributeWithMetadataName(
                WellKnownNamings.AssemblyScanAttribute,
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: (ctx, _) => (MethodDeclarationSyntax) ctx.TargetNode)
           .Where(syntax => syntax.AttributeLists.Count > 0)
           .Where(syntax => syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
           .Where(syntax => syntax.ParameterList.Parameters.Count == 0)
           .Where(syntax => syntax.TypeParameterList is null || syntax.TypeParameterList.Parameters.Count == 0);

        var compilationWithMethods = context
           .CompilationProvider
           .Combine(partialAssemblyScanMethodsProvider.Collect());

        context.RegisterSourceOutput(
            compilationWithMethods,
            GenerateCode);
    }

    private void GenerateCode(
        SourceProductionContext context,
        (Compilation Left, ImmutableArray<MethodDeclarationSyntax> Right) source)
    {
        var (compilation, methods) = source;

        ITypeFilter[] filters =
        [
            new IsAssignableToTypeFilter(),
            new HasAttributeTypeFilter()
        ];

        foreach (var method in methods.Distinct())
        {
            var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
            var methodSymbol = (ModelExtensions.GetDeclaredSymbol(semanticModel, method) as IMethodSymbol)!;

            foreach (var attributeData in methodSymbol.GetAttributes())
            {
                foreach (var filter in filters)
                {
                    filter.TryAdd(attributeData);
                }
            }

            var matchingTypes = ScanAssemblyForMatchingTypes(compilation, filters);

            if (!TryGenerateMethodImplementation(methodSymbol, matchingTypes, out var sourceText))
                continue;

            var @namespace = methodSymbol.ContainingNamespace.ToDisplayString();
            var @class = methodSymbol.ContainingType.Name;
            var methodName = methodSymbol.Name;

            context.AddSource(
                $"{@namespace}_{@class}_{methodName}_Generated.cs",
                SourceText.From(sourceText, Encoding.UTF8));
        }
    }

    private static List<INamedTypeSymbol> ScanAssemblyForMatchingTypes(
        Compilation compilation,
        ITypeFilter[] filters)
    {
        var result = new List<INamedTypeSymbol>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var typeDeclarations = root
               .DescendantNodes()
               .OfType<TypeDeclarationSyntax>();

            foreach (var typeDeclaration in typeDeclarations)
            {
                var symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, typeDeclaration);

                if (symbol is not INamedTypeSymbol typeSymbol)
                    continue;

                if (filters.All(f => f.Matches(typeSymbol)))
                    result.Add(typeSymbol);
            }
        }

        return result;
    }

    private static bool TryGenerateMethodImplementation(
        IMethodSymbol methodSymbol,
        List<INamedTypeSymbol> types,
        out string methodImplementation)
    {
        methodImplementation = "";
        var methodNamespace = methodSymbol.ContainingNamespace.ToDisplayString();

        // language=csharp
        if (!TryGenerateReturnType(methodSymbol, out var returnType))
            return false;

        methodImplementation =
            $$"""
              // <auto-generated />

              namespace {{methodNamespace}}
              {
                  public static partial class {{methodSymbol.ContainingType.Name}}
                  {
                      public static partial {{returnType}} {{methodSymbol.Name}}()
                      {
                          {{GenerateReturnStatement(types)}}
                      }
                  }
              }

              """;

        return true;
    }

    private static bool TryGenerateReturnType(
        IMethodSymbol methodSymbol,
        out string returnType)
    {
        Span<string> allowedTypes =
        [
            // language=csharp
            "System.Type[]",
            // language=csharp
            "System.Collections.Generic.IReadOnlyList<System.Type>",
            // language=csharp
            "System.Collections.Generic.IList<System.Type>",
            // language=csharp
            "System.Collections.Generic.IEnumerable<System.Type>",
            // language=csharp
            "System.Collections.Generic.ICollection<System.Type>"
        ];

        var index = allowedTypes
           .IndexOf(methodSymbol.ReturnType.ToDisplayString());

        // language=csharp
        returnType = "object";

        if (index == -1)
            return false;

        returnType = allowedTypes[index];
        return true;
    }

    private static string GenerateReturnStatement(
        List<INamedTypeSymbol> types)
    {
        if (types.Count == 0)
            // language=csharp
            return "Array.Empty<Type>();";

        var typesToReturn = string.Join(", ", types.Select(t =>
        {
            // language=csharp
            return $"typeof({t.ToDisplayString()})";
        }));

        // language=csharp
        return $$"""return new Type[] { {{typesToReturn}} };""";
    }
}
