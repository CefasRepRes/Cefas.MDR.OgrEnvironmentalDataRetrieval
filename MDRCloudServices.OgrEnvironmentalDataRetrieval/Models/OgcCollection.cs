using System.Text.Json.Serialization;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;

/// <summary>OGC Member Metadata</summary>
public class OgcMembers
{
    /// <summary>Members</summary>
    public List<OgcMember> Members { get; set; } = new();

    /// <summary>Links</summary>
    public List<OgcLink> Links { get; set; } = new();
}

/// <summary>OGC Memeber</summary>
public class OgcMember : OgcLink
{
    /// <summary>Variables</summary>
    public Dictionary<string, object> Variables { get; set; } = new();

}

/// <summary>OGC Collection metadata</summary>
public class OgcCollections
{
    /// <summary>List of collections</summary>
    public List<OgcCollection> Collections { get; set; } = new();

    /// <summary>Links</summary>
    public List<OgcLink> Links { get; set; } = new();
}

/// <summary>OGC Collection metadata</summary>
public class OgcCollection
{
    /// <summary>Collection extent</summary>
    public CollectionExtent? Extent { get; set; }

    /// <summary>Parameters</summary>
    [JsonPropertyName("parameter_names")] public Dictionary<string, object> Parameters { get; set; } = new();
    
    /// <summary>Keywords</summary>
    public List<string> Keywords { get; set; } = new();

    /// <summary>Supported reference systems</summary>
    public List<string> Crs { get; set; } = new();

    /// <summary>Supported output formats</summary>
    [JsonPropertyName("output_formats")] public List<string> OutputFormats { get; set; } = new();
    
    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Links</summary>
    public List<OgcLink> Links { get; set; } = new();
    
    /// <summary>Id</summary>
    public string? Id { get; set; }

    /// <summary>Title</summary>
    public string? Title { get; set; }

    /// <summary>Data query links</summary>
    [JsonPropertyName("data_queries")] public Dictionary<string, LinkWrapper> DataQueries { get; set; } = new();
}

/// <summary>Link wrapper</summary>
public class LinkWrapper
{
    /// <summary>Link</summary>
    public GroupMember? Link { get; set; }
}

/// <summary>Spatial and temporal extent</summary>
public class CollectionExtent
{
    /// <summary>Vertical extent</summary>
    public VerticalExtent? Vertical { get; set; }

    /// <summary>Spatial extent</summary>
    public SpatialExtent? Spatial { get; set; }

    /// <summary>Temporal extent</summary>
    public TemporalExtent? Temporal { get; set; }
}

/// <summary>Base class for extents</summary>
public class ExtentBase
{
    /// <summary>Extent name</summary>
    public string? Name { get; set; }

    /// <summary>List of intervals</summary>
    public List<List<string>>? Interval { get; set; }
}

/// <summary>Vertical extent</summary>
public class VerticalExtent : ExtentBase
{
    /// <summary>Vertical extent definition</summary>
    public string Vrs { get; set; } = "VERTCS[\"WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PARAMETER[\"Vertical_Shift\",0.0],PARAMETER[\"Direction\",1.0],UNIT[\"Meter\",1.0]],AXIS[\"Up\",UP]";
}

/// <summary>Spatial extent</summary>
public class SpatialExtent : ExtentBase
{
    /// <summary>Spatial extent definition</summary>
    public string Crs { get; set; } = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";
    
    /// <summary>Bounding box</summary>
    [JsonPropertyName("bbox")] public double?[]? BoundingBox { get; set; }
}

/// <summary>Temporal Extent</summary>
public class TemporalExtent : ExtentBase
{
    /// <summary>Temporal extent definition</summary>
    public string Trs { get; set; } = "TIMECRS[\"DateTime\",TDATUM[\"Gregorian Calendar\"],CS[TemporalDateTime,1],AXIS[\"Time (T)\",future]";
}
