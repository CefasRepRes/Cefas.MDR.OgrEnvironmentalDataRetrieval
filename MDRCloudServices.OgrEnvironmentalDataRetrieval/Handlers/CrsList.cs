using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Query to get list of CRSs supported by the system</summary>
public record CrsListQuery() : IRequest<List<Crs>>;

/// <summary>Handler to get list of CRSs supported by the system</summary>
public class CrsListHandler : IRequestHandler<CrsListQuery, List<Crs>>
{
    private readonly IDatabase _db;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    public CrsListHandler(IDatabase db)
    {
        _db = db;
    }

    /// <summary>Handle query to get list of CRSs supported by the system</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<Crs>> Handle(CrsListQuery request, CancellationToken cancellationToken)
    {
        // Limit to common reference systems as if we list all we crash the browser due to complexity
        var query = new Query("Recordsets.SpatialReferenceSystems")
            .Select(new string[] { "Name", "Projection AS Wkt" })
            .WhereIn<int>("Srid", new int[] { 3857, 4326, 4322, 27700 });
        
        var list = await _db.FetchAsync<Crs>(query);
        list.Insert(0, new Crs() { Name = "native", Wkt = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]" });
        return list;
    }
}
