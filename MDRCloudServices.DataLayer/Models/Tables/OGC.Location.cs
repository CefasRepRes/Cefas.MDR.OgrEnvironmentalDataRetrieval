using System.Runtime.Serialization;
using NPoco;

namespace MDRCloudServices.DataLayer.Models.Tables;

[TableName("OGC.Locations")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
public class OgcLocaton
{
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public string? Name { get; set; }
    [Column, DataMember] public byte[]? Geometry { get; set; }
    [Column, DataMember] public int Srid { get; set; }
}
