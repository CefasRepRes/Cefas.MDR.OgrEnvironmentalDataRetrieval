using System.Runtime.Serialization;
using Newtonsoft.Json;
using NPoco;

namespace MDRDB.MDR;

[TableName("MDR.Keywords")]
[PrimaryKey("Id")]
[ExplicitColumns]
[DataContract]
public sealed class Keyword : IEquatable<Keyword>
{
    // Table Columns
    [Column, DataMember] public int Id { get; set; }
    [Column, DataMember] public string Name { get; set; } = string.Empty;
    [Column, DataMember] public string? Vocabulary { get; set; }
    [Column, DataMember] public string? DisplayName { get; set; }
    [Column, DataMember] public string? VocabularyCode { get; set; }
    [Column, DataMember] public string? ExportName { get; set; }
    [Column, DataMember] public string? ExportCode { get; set; }
    [Column, DataMember] public DateTime? StartDate { get; set; }
    [Column, DataMember] public DateTime? EndDate { get; set; }
    [Column, DataMember] public int Order { get; set; }

    // Navigation properties
    [DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? DefinitionLink { get; set; }

    public bool Equals(Keyword? other)
    {
        return (other != null) && (
                Name == other.Name &&
                DisplayName == other.DisplayName &&
                VocabularyCode == other.VocabularyCode &&
                ExportName == other.ExportName &&
                ExportCode == other.ExportCode &&
                Order == other.Order
        );
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Keyword);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCode.Combine(Id, Name, Vocabulary, DisplayName, VocabularyCode, ExportName),
            HashCode.Combine(ExportCode, StartDate, EndDate, Order, DefinitionLink)
        );
    }
}
