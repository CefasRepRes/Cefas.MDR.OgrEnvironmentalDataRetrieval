using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get status of collections</summary>
public record CollectionStatusQuery() : IRequest<List<OgcCollectionStatus>>;

/// <summary>Handler for collection status</summary>
public class CollectionStatusHandler : IRequestHandler<CollectionStatusQuery, List<OgcCollectionStatus>>
{
    private readonly IDatabase _db;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    public CollectionStatusHandler(IDatabase db)
    {
        _db = db;
    }

    /// <summary>Handle request</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<OgcCollectionStatus>> Handle(CollectionStatusQuery request, CancellationToken cancellationToken)
    {
        var locations = new Query("OGC.LocationCache")
            .As("c")
            .SelectRaw("Count(*) AS CachedLocations, RecordsetId")
            .GroupBy(new string[] { "RecordsetId" });

        var query = new Query("Recordsets.Recordsets AS r")
            .Select(new string[] { "r.Id", "r.Name", "c.CachedLocations" })
            .LeftJoin(locations, j => j.On("r.Id", "c.RecordsetId"))
            .Where("r.PublishToOgcEdr", true);

        return await _db.FetchAsync<OgcCollectionStatus>(query);
    }
}
