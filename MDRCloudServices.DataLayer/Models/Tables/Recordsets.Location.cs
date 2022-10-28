using MDRCloudServices.DataLayer.Models;
using Newtonsoft.Json;
using NPoco;
using System.Runtime.Serialization;

namespace MDRDB.Recordsets;

[TableName("Recordsets.Locations")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
public class Location
{
    // Table columns
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public string Type { get; set; } = string.Empty;
    [Column, DataMember] public string Name { get; set; } = string.Empty;
    [Column, DataMember] public string? Description { get; set; }
    [Column, DataMember] public bool AcceptsTables { get; set; }
    [Column, DataMember] public bool AcceptsBlobs { get; set; }
    [Column, DataMember] public bool IsExternal { get; set; }
    [Column, DataMember] public bool IsThisDatabase { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Details { get; set; } = string.Empty;
    [Column, DataMember] public bool ReadOnly { get; set; }
    [Column, DataMember] public string TableNamePrefix { get; set; } = string.Empty;
    [Column, DataMember] public string Schema { get; set; } = string.Empty;
    [Column, DataMember] public bool Georeferencable { get; set; }

    // Navigation properties
    private DBConnectionDetails? _connectiondetails;
    [ResultColumn]
    public DBConnectionDetails? ConnectionDetails
    {
        // We present the decoded json object out as a property of the location, but it isn't a 'real' property, 
        // so we will keep the details property (the one we actually persist) up to date through all of the 
        // changes we are making to the connection details object
        get
        {
            if (Details != null)
            {
                _connectiondetails = JsonConvert.DeserializeObject<DBConnectionDetails>(Details);
            }
            return _connectiondetails;
        }
        set
        {
            _connectiondetails = value;
            Details = JsonConvert.SerializeObject(value);
        }
    }
}
