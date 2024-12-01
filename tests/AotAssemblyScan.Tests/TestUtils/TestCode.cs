using System.Diagnostics.CodeAnalysis;

namespace AotAssemblyScan.Tests.TestUtils;

public static class TestCode
{
    [StringSyntax("csharp")]
    public const string MarkerInterface = "public interface IMarker;";

    [StringSyntax("csharp")]
    public const string MarkerInterface2 = "public interface IMarker2;";

    [StringSyntax("csharp")]
    public const string MarkerAttribute = "public sealed class MarkerAttribute : System.Attribute;";

    [StringSyntax("csharp")]
    public const string Marker2Attribute = "public sealed class Marker2Attribute : System.Attribute;";

    [StringSyntax("csharp")]
    public const string MarkedWithInterfaceClass =
        """
        namespace Company.Project.Classes;
        
        public class MarkedWithIntefaceClass : IMarker;
        """;

    [StringSyntax("csharp")]
    public const string MarkedWith2InterfacesClass =
        """
        namespace Company.Project.Classes;
        
        public class MarkedWith2InterfacesClass : IMarker, IMarker2;
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

    [StringSyntax("csharp")]
    public const string MarkedWithAttributeClass =
        """
        namespace Company.Project.Classes;

        [Marker]
        public class MarkedWithAttributeClass;
        """;

    [StringSyntax("csharp")]
    public const string MarkedWith2AttributesClass =
        """
        namespace Company.Project.Classes;

        [Marker]
        [Marker2]
        public class MarkedWith2AttributesClass;
        """;

    [StringSyntax("csharp")]
    public const string MarkedWithAttributeRecord =
        """
        namespace Company.Project.Records;

        [Marker]
        public record MarkedWithAttributeRecord;
        """;

    [StringSyntax("csharp")]
    public const string MarkedWithAttributeStruct =
        """
        namespace Company.Project.Structs;

        [Marker]
        public struct MarkedWithAttributeStruct;
        """;
}
