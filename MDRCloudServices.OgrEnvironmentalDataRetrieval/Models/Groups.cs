using System.Text.Json.Serialization;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;

/// <summary>Groups</summary>
public class Groups
{
    /// <summary>Links</summary>
    public List<OgcLink> Links { get; set; } = new();
}

/// <summary>Group member</summary>
public class GroupMember : OgcLink
{
    /// <summary>Length</summary>
    public int? Length { get; set; }

    /// <summary>Templated</summary>
    public bool? Templated { get; set; }

    /// <summary>Variables</summary>
    public List<GroupVariable> Variables { get; set; } = new();
}

/// <summary>Group variable</summary>
public class GroupVariable
{
    /// <summary>Title</summary>
    public string? Title { get; set; }

    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Query type</summary>
    [JsonPropertyName("query_type")] public string? QueryType { get; set; }
    
    /// <summary>Coordinates</summary>
    public GroupVariableCoords? Coords { get; set; }

    /// <summary>Within units</summary>
    [JsonPropertyName("within_units")] public List<string>? WithinUnits { get; set; }

    /// <summary>Width units</summary>
    [JsonPropertyName("width_units")] public List<string>? WidthUnits { get; set; }

    /// <summary>Height units</summary>
    [JsonPropertyName("height_units")] public List<string>? HeightUnits { get; set; }

    /// <summary>Output formats</summary>
    [JsonPropertyName("output_formats")] public List<string>? OutputFormats { get; set; }

    /// <summary>Default output format</summary>
    [JsonPropertyName("default_output_format")] public string? DefaultOutputFormat { get; set; }

    /// <summary>Crs Details</summary>
    [JsonPropertyName("crs_details")] public List<Crs>? CrsDetails { get; set; }
}

/// <summary>Group variable coordinates</summary>
public class GroupVariableCoords
{
    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Type</summary>
    public string? Type { get; set; }

    /// <summary>Example</summary>
    public string? Example { get; set; }
}

/// <summary>Reference system</summary>
public class Crs
{
    /// <summary>Name</summary>
    [JsonPropertyName("crs")] public string Name { get; set; } = string.Empty;
    
    /// <summary>Well Known Text</summary>
    public string? Wkt { get; set; }
}
