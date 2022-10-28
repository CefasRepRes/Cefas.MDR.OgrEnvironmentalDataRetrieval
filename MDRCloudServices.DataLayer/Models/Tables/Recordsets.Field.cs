using MDRCloudServices.Interfaces;
using Newtonsoft.Json;
using NPoco;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MDRDB.Recordsets;

[TableName("Recordsets.Fields")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
[KnownType(typeof(Field))]
public class Field : IField
{
    // Database columns
    [Column, DataMember] public int Id { get; set; }
    [Column, IgnoreDataMember] public int RecordsetId { get; set; }
    [Column, DataMember] public string Name { get; set; } = string.Empty;
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? Description { get; set; }
    [Column, DataMember] public string Type { get; set; } = string.Empty;
    [Column, DataMember] public string ColumnName { get; set; } = string.Empty;
    [Column, DataMember] public int? Sequence { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? Units { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? Vocabulary { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public decimal? MinValue { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public decimal? MaxValue { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? Pattern { get; set; }

    // Navigation properties
    [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public IEnumerable<IFilterList>? FilterTypes { get; set; }
}
