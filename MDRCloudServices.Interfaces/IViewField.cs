using NPoco;

namespace MDRCloudServices.Interfaces;

public interface IViewField
{
    Task<IErrorResponse> ValidateAsync(IDatabase db, int recordsetId);
    int FieldId { get; set; }
    int? JoinField { get; set; }
    Task<string?> ColumnNameAsync(IDatabase db);
}
