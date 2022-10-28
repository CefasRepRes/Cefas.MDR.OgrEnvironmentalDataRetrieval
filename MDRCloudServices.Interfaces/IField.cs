namespace MDRCloudServices.Interfaces;

public interface IField
{
    int Id { get; set; }
    int RecordsetId { get; set; }
    int? Sequence { get; set; }
    string Type { get; set; }
    string ColumnName { get; set; }
    string Name { get; set; }
    public IEnumerable<IFilterList>? FilterTypes { get; set; }
}
