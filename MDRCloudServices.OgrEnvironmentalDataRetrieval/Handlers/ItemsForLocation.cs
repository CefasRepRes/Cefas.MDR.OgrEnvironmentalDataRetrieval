using System.Net;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.DataLayer.Models.Tables;
using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.Services.Handlers;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get items from a collection that match a specified location</summary>
/// <param name="collectionId"></param>
/// <param name="locationName"></param>
/// <param name="versionId"></param>
public record ItemsForLocationQuery(int collectionId, string locationName, int? versionId) : IRequest<Stream>;

/// <summary>Handler for items for location query</summary>
public class ItemsForLocationHandler : IRequestHandler<ItemsForLocationQuery, Stream>
{
    private readonly IDatabase _db;
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="m"></param>
    public ItemsForLocationHandler(IDatabase db, IMediator m)
    {
        _db = db;
        _m = m;
    }

    /// <summary>Handle request</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public async Task<Stream> Handle(ItemsForLocationQuery request, CancellationToken cancellationToken)
    {
        var recordset = await _db.FirstOrDefaultAsync<Recordset>("WHERE \"Id\" = @0 AND \"PublishToOgcEdr\" = @1", request.collectionId, true);
        if (recordset == null)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "id", "recordset" } });
        }

        var location = await _m.Send(new GetLocationForRecordsetQuery(recordset), cancellationToken);
        if (location == null)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Database Storage Location not found", "location", "recordset" } });
        }

        var tablename = await _m.Send(new TableNameForRecordsetQuery(recordset, location), cancellationToken);
        var storageDb = await _m.Send(new GetStorageDatabaseQuery(location), cancellationToken) ?? _db;

        var ogcLocation = await _db.SingleOrDefaultAsync<OgcLocaton>("WHERE \"Name\" = @0", request.locationName);
        if (ogcLocation == null)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Named Location not found", "Name", "OgcLocation" } });
        }

        var fieldSelection = await _db.FetchAsync<string>(
            new Query("recordsets.fields as f").Select("f.ColumnName")
            .Join("Recordsets.FieldTypes as t", "t.Name", "f.Type")
            .Where("t.IncludeInData", true)
            .Where("f.RecordsetId", request.collectionId));

        var query = new Query(tablename).Select(fieldSelection.ToArray())
            .Join(new Query("Ogc.LocationCache")
                    .Select("RowId")
                    .Where("RecordsetId", request.collectionId)
                    .Where("LocationId", ogcLocation.Id)
                    .As("lc"),
                j => j.On($"{tablename}.__Id", "lc.RowId"));

        if (!fieldSelection.Contains("MDR_Geometry")) query.SelectRaw("\"MDR_Geometry\".STAsBinary() AS \"MDR_Geometry\"");

        if (request.versionId == null)
        {
            query.WhereNull("DeletedVersion");
        }
        else
        {           
            if (!await _db.AnyAsync<MDRDB.Recordsets.Version>("WHERE \"Id\" = @0 AND \"RecordsetId\" = @1", recordset.Id, request.versionId))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Instance not found", "VersionId", "recordset" } });
            }
            query.Where(q => q
                .Where(x => x.Where("CreatedVersion", "<=", request.versionId).Where("DeletedVersion", ">=", request.versionId))
                .OrWhere(x => x.Where("CreatedVersion", "<=", request.versionId).WhereNull("DeletedVersion"))
                .OrWhere(x => x.WhereNull("CreatedVersion").WhereNull("DeletedVersion"))
            );
        }

        var results = storageDb.QueryAsync<Dictionary<string, object>>(query);

        return await _m.Send(new FormatAsGeoJsonQuery(request.collectionId, results), cancellationToken);
    }
}
