using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AotAssemblyScan;

[Generator]
public sealed class AssemblyScanGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodDeclarations = context.SyntaxProvider
           .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is MethodDeclarationSyntax method && method.AttributeLists.Count > 0,
                transform: static (context, _) => (MethodDeclarationSyntax) context.Node)
           .Where(static method => method != null);

        var compilationAndMethods = context.CompilationProvider.Combine(methodDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndMethods, (ctx, source) =>
        {
            var (compilation, methods) = source;

            foreach (var method in methods.Distinct())
            {
                var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
                var methodSymbol = semanticModel.GetDeclaredSymbol(method);

                if (methodSymbol == null || !HasAttribute(methodSymbol, "AssemblyScanAttribute"))
                    continue;

                var attributes = ExtractGenericTypeAttributes(methodSymbol);
                var matchingTypes = ScanAssemblyForMatchingTypes(compilation, attributes);
                var sourceText = GenerateMethodImplementation(methodSymbol, matchingTypes);

                // TODO: add namespace and type name
                ctx.AddSource($"{methodSymbol.Name}_Generated.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private static bool HasAttribute(ISymbol symbol, string attributeName)
    {
        return symbol
           .GetAttributes()
           .Any(attr => attr.AttributeClass?.Name == attributeName);
    }

    private static List<(string AttributeKind, INamedTypeSymbol TypeSymbol)> ExtractGenericTypeAttributes(
        ISymbol methodSymbol)
    {
        return methodSymbol
           .GetAttributes()
           .Where(attr =>
                attr.AttributeClass != null &&
                (attr.AttributeClass.Name.StartsWith("HasAttributeAttribute") ||
                 attr.AttributeClass.Name.StartsWith("ImplementsAttribute")) &&
                attr.AttributeClass.IsGenericType)
           .Select(attr => (attr.AttributeClass!.Name, attr.AttributeClass.TypeArguments[0] as INamedTypeSymbol))
           .Where(pair => pair.Item2 != null)
           .ToList()!;
    }

    private static List<INamedTypeSymbol> ScanAssemblyForMatchingTypes(
        Compilation compilation,
        List<(string AttributeKind, INamedTypeSymbol TypeSymbol)> filters)
    {
        var result = new List<INamedTypeSymbol>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var typeDeclarations = root.DescendantNodes()
               .OfType<TypeDeclarationSyntax>();

            foreach (var typeDeclaration in typeDeclarations)
            {
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

                if (typeSymbol == null)
                    continue;

                bool matchesAttributes = filters
                   .Where(f => f.AttributeKind.StartsWith("HasAttribute"))
                   .All(f => typeSymbol.GetAttributes()
                       .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, f.TypeSymbol)));

                bool matchesInterfaces = filters
                   .Where(f => f.AttributeKind.StartsWith("Implements"))
                   .All(f => typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, f.TypeSymbol)));

                if (matchesAttributes && matchesInterfaces)
                {
                    result.Add(typeSymbol);
                }
            }
        }

        return result;
    }

    private static string GenerateMethodImplementation(ISymbol methodSymbol, List<INamedTypeSymbol> types)
    {
        var methodNamespace = methodSymbol.ContainingNamespace.ToDisplayString();
        var typeList = string.Join(", ", types.Select(t => $"typeof({t.ToDisplayString()})"));

        return $$"""

                 using System;
                 using System.Collections.Generic;

                 namespace {{methodNamespace}}
                 {
                     public static partial class {{methodSymbol.ContainingType.Name}}
                     {
                         public static partial IReadOnlyList<Type> {{methodSymbol.Name}}()
                         {
                             return new Type[] { {{typeList}} };
                         }
                     }
                 }

                 """;
    }
}
