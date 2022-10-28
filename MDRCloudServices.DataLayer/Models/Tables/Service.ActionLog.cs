using System.Runtime.Serialization;
using NPoco;

namespace MDRDB.Service;

[TableName("Service.ActionLog")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
public class ActionLog
{
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public string Method { get; set; } = string.Empty;
    [Column, DataMember] public string Url { get; set; } = string.Empty;
    [Column, DataMember] public string IpAddress { get; set; } = string.Empty;
    [Column, DataMember] public DateTime Date { get; set; }
    [Column, DataMember] public int StatusCode { get; set; }
    [Column, DataMember] public bool Internal { get; set; }
    [Column, DataMember] public int? ObjectId { get; set; }
    [Column, DataMember] public string? ObjectType { get; set; }
    [Column, DataMember] public string? ObjectVerb { get; set; }
    [Column, DataMember] public double? ResponseTime { get; set; }
    [Column, DataMember] public string? AdditionalParam { get; set; }
    [Column, DataMember] public string? UserAgent { get; set; }
    [Column, DataMember] public string? Origin { get; set; }
    [Column, DataMember] public string? Referer { get; set; }
}
