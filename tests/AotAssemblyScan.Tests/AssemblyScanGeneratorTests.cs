using AotAssemblyScan.Tests.TestUtils;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AotAssemblyScan.Tests;

public class AssemblyScanGeneratorTests
{
    private readonly CSharpGeneratorDriver _generatorDriver;

    public AssemblyScanGeneratorTests()
    {
        var generator = new AssemblyScanGenerator();
        _generatorDriver = CSharpGeneratorDriver.Create(generator);
    }

    [Fact]
    public void Does_not_generate_implementation_without_assembly_scan_attribute()
    {
        // Arrange

        // language=csharp
        string code =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;
            
            namespace DefaultAssembly;
            
            public static partial class AssemblyExtensions
            {
                [IsAbstract]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(code)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        result
           .GetRunResult()
           .GeneratedTrees
           .Should()
           .HaveCount(0);
    }

    [Fact]
    public void Does_not_generate_implementation_without_partial_modifier()
    {
        // Arrange

        // language=csharp
        string code =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                [IsAbstract]
                public static IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(code)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        result
           .GetRunResult()
           .GeneratedTrees
           .Should()
           .HaveCount(0);
    }

    [Fact]
    public async Task When_only_assembly_scan_attribute_specified_should_return_all_assembly_types()
    {
        // Arrange
        // language=csharp
        string methodCode =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.MarkerInterface),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithInterfaceClass),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithInterfaceRecord),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithInterfaceStruct),
                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }

    // generates code for methods with similar name

    // does not generate code for method with parameters

    // generates code only for IReadOnlyList (handle IEnumerable, ICollection, IList, etc. in the future)

    // includes the type itself in IsAssignableTo

    // finds inheritors in IsAssignableTo

    // combines IsAssignableTo using &&

    // finds types with HasAttribute

    // combines HasAttribute using &&

    // includes only abstract classes when using IsAbstract

    // filters abstract classes when using IsAbstract(false)

    // includes only interfaces when using IsInterface

    // filters interfaces when using IsInterface(false)
}
