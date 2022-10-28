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

/// <summary>Data Query</summary>
/// <param name="Id"></param>
/// <param name="versionId"></param>
/// <param name="AddGeospatialQuery"></param>
/// <param name="datetime"></param>
/// <param name="patameterNames"></param>
/// <param name="crs"></param>
/// <param name="f"></param>
public record DataQuery(int Id, string? versionId, Action<Query> AddGeospatialQuery, string? datetime, string? patameterNames, string crs, string f) : IRequest<Stream>;

/// <summary>Data query handler</summary>
public class DataHandler : IRequestHandler<DataQuery, Stream>
{
    private readonly IDatabase _db;
    private readonly IRecordsetService _recordsetService;
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="recordsetService"></param>
    /// <param name="mediator"></param>
    public DataHandler(
        IDatabase db,
        IRecordsetService recordsetService,
        IMediator mediator)
    {
        _db = db;
        _recordsetService = recordsetService;
        _m = mediator;
    }

    /// <summary>Handle data request</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public async Task<Stream> Handle(DataQuery request, CancellationToken cancellationToken)
    {
        Recordset recordset;
        try
        {
            recordset = await _db.FirstOrDefaultAsync<Recordset>("WHERE \"Id\" = @0 AND \"PublishToOgcEdr\" = @1", request.Id, true);
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

        var fieldSelection = await _db.FetchAsync<string>(
            "SELECT f.ColumnName FROM recordsets.fields f " +
            "INNER JOIN Recordsets.FieldTypes t ON t.Name = f.Type AND t.IncludeInData = 1 " +
            "WHERE f.RecordsetId = @0", request.Id);

        if (!string.IsNullOrEmpty(request.patameterNames))
        {
            var requestedFields = request.patameterNames.Split(',').Select(x => x.Trim()).ToList();
            fieldSelection = fieldSelection.Where(x => requestedFields.Contains(x)).ToList();
        }

        if (!fieldSelection.Contains("__Id")) fieldSelection.Insert(0, "__Id");

        var query = new Query(tablename).Select(fieldSelection.ToArray());
        if (!fieldSelection.Contains("MDR_Geometry")) query.SelectRaw("\"MDR_Geometry\".STAsBinary() AS \"MDR_Geometry\"");

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

        var results = storageDb.QueryAsync<Dictionary<string, object>>(query);

        return request.f.ToLowerInvariant() switch
        {
            "geojson" => await _m.Send(new FormatAsGeoJsonQuery(request.Id, results), cancellationToken),
            _ => throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Invalid format", "f", "HandleDataQuery" } })
        };
    }
}
