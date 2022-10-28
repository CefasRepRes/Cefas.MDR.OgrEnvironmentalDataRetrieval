using System.Net;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.Exceptions;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MDRCloudServices.Services.Handlers;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get list of item identifiers</summary>
/// <param name="collectionId"></param>
/// <param name="versionId"></param>
/// <param name="AddGeospatialQuery"></param>
/// <param name="datetime"></param>
public record ItemsInCollectionQuery(int collectionId, string? versionId, Action<Query> AddGeospatialQuery, string? datetime) : IRequest<List<int>>;

/// <summary>Handler for items in collection query</summary>
public class ItemsInCollectionHandler : IRequestHandler<ItemsInCollectionQuery, List<int>>
{
    private readonly IDatabase _db;
    private readonly IRecordsetService _recordsetService;
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="recordsetService"></param>
    /// <param name="mediator"></param>
    public ItemsInCollectionHandler(
    IDatabase db,
    IRecordsetService recordsetService,
    IMediator mediator)
    {
        _db = db;
        _recordsetService = recordsetService;
        _m = mediator;
    }

    /// <summary>Handle request</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public async Task<List<int>> Handle(ItemsInCollectionQuery request, CancellationToken cancellationToken)
    {
        Recordset recordset;
        try
        {
            recordset = await _db.FirstOrDefaultAsync<Recordset>("WHERE \"Id\" = @0 AND \"PublishToOgcEdr\" = @1", request.collectionId, true);
        }
        catch (NotFoundException ex)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "id", "recordset" } }, ex);
        }

        Location location;
        try
        {
            location = await _m.Send(new GetLocationForRecordsetQuery(recordset), cancellationToken);
        }
        catch (NotFoundException ex)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Location not found", "location", "recordset" } }, ex);
        }

        var tablename = await _m.Send(new TableNameForRecordsetQuery(recordset, location), cancellationToken);
        var storageDb = await _m.Send(new GetStorageDatabaseQuery(location), cancellationToken) ?? _db;
        var fields = await _recordsetService.GetFieldsForRecordset(recordset, false);
        var dateFieldTypes = await _m.Send(new DateFieldTypesQuery(), cancellationToken);
        var dateColumn = fields.First(x => dateFieldTypes.Contains(x.Type)).ColumnName;
     
        var query = new Query(tablename).Select("__Id");

        request.AddGeospatialQuery(query);
        OgcQueryBuilder.BuildDateQuery(query, request.datetime, dateColumn);
        if (string.IsNullOrEmpty(request.versionId))
        {
            query.WhereNull("DeletedVersion");
        }
        else
        {
            if (!int.TryParse(request.versionId, out int versionId))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Instance not found", "VersionId", "recordset" } });
            }
            if (!await _db.AnyAsync<MDRDB.Recordsets.Version>("WHERE \"Id\" = @0 AND \"RecordsetId\" = @1", recordset.Id, versionId))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Instance not found", "VersionId", "recordset" } });
            }
            query.Where(q => q
                .Where(x => x.Where("CreatedVersion", "<=", versionId).Where("DeletedVersion", ">=", versionId))
                .OrWhere(x => x.Where("CreatedVersion", "<=", versionId).WhereNull("DeletedVersion"))
                .OrWhere(x => x.WhereNull("CreatedVersion").WhereNull("DeletedVersion"))
            );
        }

        return await storageDb.FetchAsync<int>(query);
    }
}
