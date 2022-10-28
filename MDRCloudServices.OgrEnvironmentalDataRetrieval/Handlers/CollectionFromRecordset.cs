using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.Interfaces;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Collection from recordset query</summary>
/// <param name="Recordset"></param>
/// <param name="StorageDb"></param>
/// <param name="Fields"></param>
/// <param name="TableName"></param>
/// <param name="DateColumn"></param>
public record CollectionFromRecordsetQuery(Recordset Recordset, IDatabase StorageDb, List<IField> Fields, string TableName, string? DateColumn) : IRequest<OgcCollection>;

/// <summary>Collection from recordset handler</summary>
public class CollectionFromRecordsetHandler : IRequestHandler<CollectionFromRecordsetQuery, OgcCollection>
{
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="m"></param>
    public CollectionFromRecordsetHandler(IMediator m)
    {
        _m = m;
    }

    /// <summary>Handle collection from recordset</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OgcCollection> Handle(CollectionFromRecordsetQuery request, CancellationToken cancellationToken)
    {
        // Links and data queries are populated at the controller level to avoid circular references
        var collection = new OgcCollection
        {
            Title = request.Recordset.Name,
            Description = request.Recordset.Description,
            Parameters = request.Fields.AsQueryable().ToDictionary(x => x.Name, x => new object()),
            OutputFormats = new() { "GeoJSON" },
            Extent = await GetCollectionExtent(request.StorageDb, request.TableName, request.DateColumn),
            Id = request.Recordset.Id.ToString(),
            Crs = (await _m.Send(new CrsListQuery(), cancellationToken)).Select(x => x.Name).ToList(), 
            Keywords = await _m.Send(new KeywordsForCollectionQuery(request.Recordset.Id), cancellationToken)
        };

        return collection;
    }

    /// <summary>Get spatial extent</summary>
    /// <param name="storagedb"></param>
    /// <param name="tablename"></param>
    /// <returns></returns>
    public static async Task<double?[]> GetSpatialExtentAsync(IDatabase storagedb, string tablename)
    {
        var cte_envelope = new Query(tablename).SelectRaw("MDR_Geometry.STEnvelope() AS envelope");
        var cte_corner = new Query("cte_envelope").SelectRaw("envelope.STPointN(1) as point")
            .UnionAll(new Query("cte_envelope").SelectRaw("envelope.STPointN(3)"));
        var query = new Query("cte_corner")
            .With("cte_envelope", cte_envelope)
            .With("cte_corner", cte_corner)
            .SelectRaw("MIN(point.STX) as MinX, MIN(point.STY) MinY, MAX(point.STX) as MaxX, MAX(point.STY) as MaxY");
        
        var extent = await storagedb.QueryAsync<BoundingBox>(query).FirstOrDefaultAsync();

        if (extent == null) return Array.Empty<double?>(); 

        return new double?[] { extent.MinX, extent.MinY, extent.MaxX, extent.MaxY };
    }

    /// <summary>Get temporal extent</summary>
    /// <param name="storagedb"></param>
    /// <param name="tablename"></param>
    /// <param name="columnname"></param>
    /// <returns></returns>
    public static async Task<List<string>> GetTemporalExtentAsync(IDatabase storagedb, string tablename, string? columnname)
    {
        if (storagedb == null || string.IsNullOrEmpty(tablename) || string.IsNullOrEmpty(columnname))
        {
            return new();
        }

        var query = new Query(tablename).SelectRaw($"MIN(\"{columnname}\") AS Minimum, MAX(\"{columnname}\") AS Maximum");
            
        var dates = await storagedb.FetchAsync<MinMax>(query);
        return dates.Select(x => $"{x.Minimum:o}/{x.Maximum:o}").ToList();
    }

    /// <summary>Get collection extent</summary>
    /// <param name="storagedb"></param>
    /// <param name="tablename"></param>
    /// <param name="datecolumn"></param>
    /// <returns></returns>
    public static async Task<CollectionExtent> GetCollectionExtent(IDatabase storagedb, string tablename, string? datecolumn)
    {
        return new()
        {
            Spatial = new() { BoundingBox = await GetSpatialExtentAsync(storagedb, tablename) },
            Temporal = new() { Interval = new() { await GetTemporalExtentAsync(storagedb, tablename, datecolumn) } },
            Vertical = new()
        };
    }
}

internal class MinMax
{
    public DateTime? Minimum  { get; set; }
    public DateTime? Maximum { get; set; }
}
