namespace MDRCloudServices.Interfaces;

public interface IFilterList
{
    public string FieldType { get; set; }
    public string? LongName { get; set; }
    public string ShortName { get; set; }
    public int? Operands { get; set; }
    public string? OperandType { get; set; }
}
