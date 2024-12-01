using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AotAssemblyScan.Tests.TestUtils;

public static class DefaultCompilation
{
    public static CSharpCompilation Create(
        string assemblyName,
        IEnumerable<SyntaxTree> syntaxTrees)
    {
        return CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            [
                MetadataReference.CreateFromFile(typeof(AssemblyScanAttribute).Assembly.Location),
                ..Basic.Reference.Assemblies.Net80.References.All
            ],
            new(OutputKind.DynamicallyLinkedLibrary));
    }
}
