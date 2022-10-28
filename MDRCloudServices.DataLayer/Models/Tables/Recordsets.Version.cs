using NPoco;
using System;
using System.Runtime.Serialization;

namespace MDRDB.Recordsets;

[TableName("Recordsets.Versions")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
[KnownType(typeof(Version))]
public class Version
{
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public int RecordsetId { get; set; }
    [Column, DataMember] public DateTime Date { get; set; }
    [Column, DataMember] public bool IsReady { get; set; }
    [Column, DataMember] public int RecordsAdded { get; set; }
    [Column, DataMember] public int RecordsRemoved { get; set; }
    [Column, DataMember] public int ActiveRecords { get; set; }
    [Column, DataMember] public int TotalRecords { get; set; }
}
