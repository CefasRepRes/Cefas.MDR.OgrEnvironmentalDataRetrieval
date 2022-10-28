using MDRCloudServices.Interfaces;
using Newtonsoft.Json;
using NPoco;
using System.Runtime.Serialization;

namespace MDRDB.Recordsets;

[TableName("Shared.FilterList")]
[ExplicitColumns]
[DataContract]
[KnownType(typeof(FilterList))]
public class FilterList : IFilterList
{
    [Column, IgnoreDataMember] public string FieldType { get; set; } = string.Empty;
    [Column, DataMember] public string? LongName { get; set; }
    [Column, DataMember] public string ShortName { get; set; } = string.Empty;
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Operands { get; set; }
    [Column, DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? OperandType { get; set; }
}
