using NPoco;

namespace MDRCloudServices.Interfaces;

public interface IFieldSelection
{
    bool ReverseSort { get; set; }
    int? SortColumn { get; set; }

    List<IViewField> ViewFields { get; set; }

    Task AddSort(Sql sql, IDatabase db);

    Task<List<IField>> GetFields(IDatabase db);

    Task<string> SelectionList(IDatabase db, int recordsetId);

    Task<IErrorResponse> ValidateAsync(IDatabase db, int recordsetId);
}
