using NPoco;
using System.Runtime.Serialization;

namespace MDRDB.Recordsets;

[TableName("Recordsets.Licences")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
public class Licence
{
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public string Name { get; set; } = string.Empty;
    [Column, DataMember] public string Href { get; set; } = string.Empty;
}
