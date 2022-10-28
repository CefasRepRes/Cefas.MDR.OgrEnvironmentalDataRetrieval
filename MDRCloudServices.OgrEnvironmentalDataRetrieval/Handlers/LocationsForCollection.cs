using System.Net;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Query locations from collection id</summary>
/// <param name="id"></param>
/// <param name="instanceId"></param>
/// <param name="bbox"></param>
/// <param name="datetime"></param>
public record LocationsForCollectionQuery(int id, int? instanceId, double?[]? bbox, string? datetime) : IRequest<List<string>>;

/// <summary>Handle query locations from collection id</summary>
public class LocationsForCollectionHandler : IRequestHandler<LocationsForCollectionQuery, List<string>>
{
    private readonly IDatabase _db;
    private readonly IMediator _m;
    private readonly IRecordsetService _recordsetService;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="m"></param>
    /// <param name="recordsetService"></param>
    public LocationsForCollectionHandler(IDatabase db, IMediator m, IRecordsetService recordsetService)
    {
        _db = db;
        _m = m;
        _recordsetService = recordsetService;
    }

    /// <summary>Handle request</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<string>> Handle(LocationsForCollectionQuery request, CancellationToken cancellationToken)
    {
        var recordset = await _db.FirstOrDefaultAsync<Recordset>(new Query()
            .GenerateSelect(typeof(Recordset))
            .Where("PublishToOgcEdr", true)
            .Where("Id", request.id));

        if (recordset == null)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "id", "recordset" } });
        }

        var fields = await _recordsetService.GetFieldsForRecordset(recordset, false);
        var dateFieldTypes = await _m.Send(new DateFieldTypesQuery(), cancellationToken);
        var dateColumn = fields.Where(x => dateFieldTypes.Contains(x.Type)).Select(x => x.ColumnName).FirstOrDefault();

        var query = new Query("OGC.LocationCache AS C")
            .Distinct()
            .SelectRaw("Name")
            .Join("OGC.Locations AS L", "L.Id", "C.LocationId")
            .Where("C.RecordsetId", recordset.Id);

        if (request.instanceId != null)
        {
            query.Where("C.CreatedVersion", "<=", request.instanceId);
            query.Where(x => x.Where("C.DeletedVersion", ">=", request.instanceId).OrWhereNull("C.DeletedVersion"));
        }
        else
        {
            query.WhereNull("C.DeletedVersion");
        }

        if (request.bbox != null && request.bbox.Length > 0)
        {
            var polygon = OgcQueryBuilder.BuildBoundingBoxPolygon(request.bbox);
            OgcQueryBuilder.BuildGeometryQuery(query, polygon, "Geometry");
        }
        OgcQueryBuilder.BuildDateQuery(query, request.datetime, dateColumn);

        return await _db.FetchAsync<string>(query.ToSql());
    }
}
