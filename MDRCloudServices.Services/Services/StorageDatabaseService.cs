using MDRCloudServices.Exceptions;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using NPoco;

namespace MDRCloudServices.Services.Services;

public class StorageDatabaseService : IStorageDatabaseService
{
    private readonly IDatabase _db;

    public StorageDatabaseService(IDatabase db)
    {
        _db = db;
    }

    public IDatabase GetDatabase(Location loc)
    {
        if (loc.IsThisDatabase) return _db;
        if (loc.ConnectionDetails == null) throw new NotFoundException("Connection details missing");
        return loc.ConnectionDetails.CreateDb() ?? throw new NotFoundException("Unable to create storage database connection");
    }
}
