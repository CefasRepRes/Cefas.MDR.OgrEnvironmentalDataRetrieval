using MDRCloudServices.DataLayer.SqlKata;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get instances for collection query</summary>
/// <param name="id"></param>
public record InstancesForCollectionQuery(int id) : IRequest<List<string>>;

/// <summary>
/// Get instances for collection handler
/// </summary>
public class InstancesForCollectionHandler : IRequestHandler<InstancesForCollectionQuery, List<string>>
{
    private readonly IDatabase _db;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    public InstancesForCollectionHandler(IDatabase db)
    {
        _db = db;
    }

    /// <summary>Handle instances for collection query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<string>> Handle(InstancesForCollectionQuery request, CancellationToken cancellationToken)
    {
        var items = await _db.FetchAsync<int>(new Query().ForType<MDRDB.Recordsets.Version>().SelectRaw("Id").Where("RecordsetId", request.id));
        return items.Select(x => x.ToString()).ToList();
    }
}
