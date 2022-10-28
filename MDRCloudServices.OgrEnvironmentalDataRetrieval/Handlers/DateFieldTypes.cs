using MDRCloudServices.DataLayer.SqlKata;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Date field types query</summary>
public record DateFieldTypesQuery() : IRequest<List<string>>;

/// <summary>Date field types handler</summary>
public class DateFieldTypesQueryHandler : IRequestHandler<DateFieldTypesQuery, List<string>>
{
    private readonly IDatabase _db;

    /// <summary>Default Constructor</summary>
    /// <param name="db"></param>
    public DateFieldTypesQueryHandler(IDatabase db)
    {
        _db = db;
    }

    /// <summary>Handle date field types query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<string>> Handle(DateFieldTypesQuery request, CancellationToken cancellationToken)
    {
        return await _db.FetchAsync<string>(
            new Query().FromRaw("Recordsets.FieldTypes").Select("Name").WhereIn("SqlColumnDef", new string[]
        {
            "[date]",
            "[datetime]",
            "[datetime2]"
        }));
    }
}


