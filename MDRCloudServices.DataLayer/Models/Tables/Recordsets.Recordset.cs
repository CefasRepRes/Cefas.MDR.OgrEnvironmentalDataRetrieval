using System.Runtime.Serialization;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.Enums;
using MDRCloudServices.Interfaces;
using Newtonsoft.Json;
using NPoco;

namespace MDRDB.Recordsets;

[TableName("Recordsets.Recordsets")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
[KnownType(typeof(Recordset))]
public class Recordset
{
    // Database Columns
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public string Name { get; set; } = string.Empty;
    [Column, DataMember] public string? Description { get; set; }
    [Column, DataMember] public int Location { get; set; }
    [Column, DataMember] public int HoldingId { get; set; }
    [Column, DataMember] public bool Tabular { get; set; }
    [Column, DataMember] public int? SourceRecordset { get; set; }
    [Column, DataMember] public int? SourceVersion { get; set; }
    [Column, DataMember] public string? CreationStep { get; set; }
    [Column, DataMember] public string? KeyField { get; set; }
    [Column, DataMember] public string? QuoteCharacter { get; set; }
    [Column, DataMember] public string? NullString { get; set; }
    [Column, DataMember] public bool External { get; set; }
    [Column, DataMember] public int? SRID { get; set; }
    [Column, DataMember] public bool Versioned { get; set; }
    [Column, DataMember] public string? TableName { get; set; }
    [Column, DataMember] public int Priority { get; set; }
    [Column, DataMember] public int Licence { get; set; }
    [Column, DataMember] public RecordsetMode Mode { get; set; }
    [Column, DataMember] public bool Draft { get; set; }
    [Column, DataMember] public bool Hidden { get; set; }
    [Column, DataMember] public int? DeletedVersion { get; set; }
    [Column, DataMember] public string? DeletedBy { get; set; }
    [Column, DataMember] public bool PublishToOgcEdr { get; set; } 


    // Supplementary fields
    [ResultColumn, DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<Version> Versions { get; set; } = new();

    [ResultColumn, DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<IField> Fields { get; set; } = new();

    [ResultColumn, DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFieldSelection? FieldSelection { get; set; }

    [DataMember]
    public List<RecordsetFilter> Filters { get; set; } = new();

    [DataMember]
    public Dictionary<string, ServiceLink> Links { get; set; } = new();
}
