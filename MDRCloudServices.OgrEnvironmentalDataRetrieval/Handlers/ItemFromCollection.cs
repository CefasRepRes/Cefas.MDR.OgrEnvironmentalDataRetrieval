using System.Net;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.Services.Handlers;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get a single record from a collection by id</summary>
/// <param name="collectionId"></param>
/// <param name="itemId"></param>
public record ItemFromCollectionQuery(int collectionId, int itemId) : IRequest<Stream>;

/// <summary>Handler for item from collection query</summary>
public class ItemFromCollectionHandler : IRequestHandler<ItemFromCollectionQuery, Stream>
{
    private readonly IDatabase _db;
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="mediator"></param>
    public ItemFromCollectionHandler(
        IDatabase db,
        IMediator mediator)
    {
        _db = db;
        _m = mediator;
    }

    /// <summary>Handle item from collection query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Stream> Handle(ItemFromCollectionQuery request, CancellationToken cancellationToken)
    {
        var recordset = await _db.FirstOrDefaultAsync<Recordset>("WHERE \"Id\" = @0 AND \"PublishToOgcEdr\" = @1", request.collectionId, true);
        if (recordset == null)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "id", "recordset" } });
        }

        var location = await _m.Send(new GetLocationForRecordsetQuery(recordset), cancellationToken);
        if (location == null)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Location not found", "location", "recordset" } });
        }

        var tablename = await _m.Send(new TableNameForRecordsetQuery(recordset, location), cancellationToken);
        var storageDb = await _m.Send(new GetStorageDatabaseQuery(location), cancellationToken) ?? _db;

        var fieldSelection = await _db.FetchAsync<string>(
            new Query("recordsets.fields as f").Select("f.ColumnName")
            .Join("Recordsets.FieldTypes as t","t.Name", "f.Type")
            .Where("t.IncludeInData", true)
            .Where("f.RecordsetId", request.collectionId));

        var query = new Query(tablename).Select(fieldSelection.ToArray())
            .Where("__Id", request.itemId)
            .WhereNull("DeletedVersion");

        if (!fieldSelection.Contains("MDR_Geometry")) query.SelectRaw("\"MDR_Geometry\".STAsBinary() AS \"MDR_Geometry\"");

        var results = storageDb.QueryAsync<Dictionary<string, object>>(query);

        return await _m.Send(new FormatAsGeoJsonQuery(request.collectionId, results), cancellationToken);
    }
}
