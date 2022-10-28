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

/// <summary>Collections query</summary>
/// <param name="bbox"></param>
/// <param name="datetime"></param>
public record CollectionsQuery(double?[]? bbox, string? datetime) : IRequest<OgcCollections>;

/// <summary>Collections query handler</summary>
public class CollectionsQueryHandler : IRequestHandler<CollectionsQuery, OgcCollections>
{
    private readonly IDatabase _db;
    private readonly IRecordsetService _recordsetService;
    private readonly IMediator _m;
    
    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="recordsetService"></param>
    /// <param name="mediator"></param>
    public CollectionsQueryHandler(
        IDatabase db, 
        IRecordsetService recordsetService, 
        IMediator mediator)
    {
        _db = db;
        _recordsetService = recordsetService;
        _m = mediator;
    }

    /// <summary>Handle query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<OgcCollections> Handle(CollectionsQuery request, CancellationToken cancellationToken)
    {
        var recordsets = await _db.QueryAsync<Recordset>().Where(x => x.PublishToOgcEdr).ToList();
        var collections = new OgcCollections();
        foreach (var recordset in recordsets)
        {
            var include = true;

            var location = await _m.Send(new GetLocationForRecordsetQuery(recordset), cancellationToken);
            if (location == null)
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Location not found", "id", "recordset" } });
            }

            var tablename = await _m.Send(new TableNameForRecordsetQuery(recordset, location), cancellationToken);
            var storageDb = await _m.Send(new GetStorageDatabaseQuery(location), cancellationToken) ?? _db;
            var fields = await _recordsetService.GetFieldsForRecordset(recordset, false);
            var builder = new Query().FromRaw(tablename).Select("__Id").Limit(1).WhereNull("DeletedVersion");
            var dateFieldTypes = await _m.Send(new DateFieldTypesQuery(), cancellationToken);
            var dateColumn = fields.Where(x => dateFieldTypes.Contains(x.Type)).Select(x => x.ColumnName).FirstOrDefault();

            if (request.bbox != null && request.bbox.Length > 0)
            {
                OgcQueryBuilder.BuildGeometryQuery(builder, OgcQueryBuilder.BuildBoundingBoxPolygon(request.bbox));
            }
            OgcQueryBuilder.BuildDateQuery(builder, request.datetime, dateColumn);
                        
            include = null != await _db.ExecuteScalarAsync<int?>(builder);

            if (include)
            {
                var collection = await _m.Send(new CollectionFromRecordsetQuery(recordset, storageDb, fields, tablename, dateColumn), cancellationToken);
                collections.Collections.Add(collection);
            }
        }

        return collections;
    }
}

