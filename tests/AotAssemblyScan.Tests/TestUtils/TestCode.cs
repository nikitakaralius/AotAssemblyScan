using System.Diagnostics.CodeAnalysis;

namespace AotAssemblyScan.Tests.TestUtils;

public static class TestCode
{
    [StringSyntax("csharp")]
    public const string MarkerInterface = "public interface IMarker;";

    [StringSyntax("csharp")]
    public const string MarkerAttribute = "public sealed class MarkerAttribute : Attribute;";

    [StringSyntax("csharp")]
    public const string MarkedWithInterfaceClass =
        """
        namespace Company.Project.Classes;
        
        public class MarkedWithIntefaceClass : IMarker;
        """;

    [StringSyntax("csharp")]
    public const string MarkedWithInterfaceRecord =
        """
        namespace Company.Project.Records;
        
        public record MarkedWithIntefaceRecord : IMarker;
        """;

    [StringSyntax("csharp")]
    public const string MarkedWithInterfaceStruct =
        """
        namespace Company.Project.Structs;
        
        public struct MarkedWithIntefaceStruct : IMarker;
        """;
}
