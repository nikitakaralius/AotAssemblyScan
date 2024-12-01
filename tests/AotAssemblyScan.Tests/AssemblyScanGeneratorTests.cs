using AotAssemblyScan.Tests.TestUtils;
using FluentAssertions;
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
    public void Does_not_generate_code_for_method_with_parameters()
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
                public static partial IReadOnlyList<Type> GetMarkedTypes(int number);
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
        result
           .GetRunResult()
           .GeneratedTrees
           .Should()
           .HaveCount(0);
    }

    [Fact]
    public void Does_not_generate_code_for_generic_method()
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
                public static partial IReadOnlyList<Type> GetMarkedTypes<T>();
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

    [Fact]
    public async Task Generates_code_for_methods_with_similar_name()
    {
        // Arrange
        // language=csharp
        string methodCode1 =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly1;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        // language=csharp
        string methodCode2 =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly2;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(methodCode1),
                CSharpSyntaxTree.ParseText(methodCode2)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);

    }

    [Theory]
    [InlineData("Type[]")]
    [InlineData("IReadOnlyList<Type>")]
    [InlineData("IList<Type>")]
    [InlineData("IEnumerable<Type>")]
    [InlineData("ICollection<Type>")]
    public async Task Generates_code_for_different_return_types(string returnType)
    {
        // Arrange
        // language=csharp
        string methodCode =
            $$"""
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                public static partial {{returnType}} GetMarkedTypes();
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
           .UseParameters(returnType)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task Includes_generic_type_from_IsAssignableTo_attribute()
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
                [IsAssignableTo<IMarker>]
                public static partial Type[] GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.MarkerInterface),
                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task Finds_generic_type_inheritors_from_IsAssignableTo_attribute()
    {
        // Arrange
        // language=csharp
        string methodCode =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly1;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                [IsAssignableTo<IMarker>]
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
                CSharpSyntaxTree.ParseText(methodCode),
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);

    }

    [Fact]
    public async Task Combines_multiple_IsAssignableTo_attributes_with_the_logical_AND_operator()
    {
        // Arrange
        // language=csharp
        string methodCode =
            """
            using System;
            using System.Collections.Generic;
            using AotAssemblyScan;

            namespace DefaultAssembly1;

            public static partial class AssemblyExtensions
            {
                [AssemblyScan]
                [IsAssignableTo<IMarker>]
                [IsAssignableTo<IMarker2>]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.MarkerInterface),
                CSharpSyntaxTree.ParseText(TestCode.MarkerInterface2),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWith2InterfacesClass),
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

    [Fact]
    public async Task Finds_types_marked_with_attribute_specified_in_HasAttribute()
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
                [HasAttribute<MarkerAttribute>]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.MarkerAttribute),
                CSharpSyntaxTree.ParseText(TestCode.Marker2Attribute),

                CSharpSyntaxTree.ParseText(TestCode.MarkedWithAttributeClass),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWith2AttributesClass),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithAttributeRecord),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithAttributeStruct),

                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task Combines_multiple_HasAttribute_attributes_with_the_logical_AND_operator()
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
                [HasAttribute<MarkerAttribute>]
                [HasAttribute<Marker2Attribute>]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.MarkerAttribute),
                CSharpSyntaxTree.ParseText(TestCode.Marker2Attribute),

                CSharpSyntaxTree.ParseText(TestCode.MarkedWithAttributeClass),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWith2AttributesClass),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithAttributeRecord),
                CSharpSyntaxTree.ParseText(TestCode.MarkedWithAttributeStruct),

                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);

    }

    [Fact]
    public async Task Includes_only_abstract_classes_when_using_IsAbstract()
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
                [IsAbstract]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.Class),
                CSharpSyntaxTree.ParseText(TestCode.AbstractClass),
                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task Filters_abstract_classes_when_using_IsAbstract_false()
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
                [IsAbstract(false)]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.Class),
                CSharpSyntaxTree.ParseText(TestCode.AbstractClass),
                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);

    }

    [Fact]
    public async Task Includes_only_interfaces_when_using_IsInterface()
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
                [IsInterface]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.Class),
                CSharpSyntaxTree.ParseText(TestCode.MarkerInterface),
                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }

    [Fact]
    public async Task Filters_interfaces_when_using_IsInterface_false()
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
                [IsInterface(false)]
                public static partial IReadOnlyList<Type> GetMarkedTypes();
            }
            """;

        var compilation = DefaultCompilation.Create(
            "DefaultAssembly",
            [
                CSharpSyntaxTree.ParseText(TestCode.Class),
                CSharpSyntaxTree.ParseText(TestCode.MarkerInterface),
                CSharpSyntaxTree.ParseText(methodCode)
            ]);

        // Act
        var result = _generatorDriver.RunGenerators(compilation);

        // Assert
        await Verify(result)
           .UseDirectory(TestConstants.SnapshotsDirectory);
    }
}
