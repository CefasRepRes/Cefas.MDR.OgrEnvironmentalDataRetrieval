using MDRCloudServices.Exceptions;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MDRCloudServices.Services.Handlers;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;
using NPoco;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Query collection by ID</summary>
/// <param name="id"></param>
/// <param name="bbox"></param>
/// <param name="datetime"></param>
public record CollectionByIdQuery(int id, double?[]? bbox, string? datetime) : IRequest<OgcCollection>;

/// <summary>Collection by ID handler</summary>
public class CollectionByIdHandler : IRequestHandler<CollectionByIdQuery, OgcCollection>
{
    private readonly IDatabase _db;
    private readonly IRecordsetService _recordsetService;
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="recordsetService"></param>
    /// <param name="mediator"></param>
    public CollectionByIdHandler(
        IDatabase db,
        IRecordsetService recordsetService,
        IMediator mediator)
    {
        _db = db;
        _m = mediator;
        _recordsetService = recordsetService;
    }

    /// <summary>Handle collection by id query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<OgcCollection> Handle(CollectionByIdQuery request, CancellationToken cancellationToken)
    {
        var recordset = await _db.FirstOrDefaultAsync<Recordset>("WHERE \"Id\" = @0", request.id);
        var location = await _m.Send(new GetLocationForRecordsetQuery(recordset), cancellationToken);
        var tablename = await _m.Send(new TableNameForRecordsetQuery(recordset, location), cancellationToken);
        var storageDb = await _m.Send(new GetStorageDatabaseQuery(location), cancellationToken);
        var fields = await _recordsetService.GetFieldsForRecordset(recordset, false);
        var dateFieldTypes = await _m.Send(new DateFieldTypesQuery(), cancellationToken);
        var dateColumn = fields.First(x => dateFieldTypes.Contains(x.Type)).ColumnName;

        if (recordset == null) throw new NotFoundException($"Collection {request.id} not found");
        return await _m.Send(new CollectionFromRecordsetQuery(recordset, storageDb, fields, tablename, dateColumn), cancellationToken);
    }
}
