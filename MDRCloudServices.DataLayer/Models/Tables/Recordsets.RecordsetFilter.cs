using Newtonsoft.Json;
using NPoco;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MDRDB.Recordsets;

[TableName("Recordsets.RecordsetFilters")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
[KnownType(typeof(RecordsetFilter))]
public class RecordsetFilter
{
    // Table columns
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public int RecordsetId { get; set; }
    [Column, DataMember] public int FieldId { get; set; }
    [Column, DataMember] public int? FinalFieldId { get; set; }
    [Column, DataMember] public string FilterType { get; set; } = string.Empty;

    // Navigation properties
    [DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Operands { get; set; } = new();
}
