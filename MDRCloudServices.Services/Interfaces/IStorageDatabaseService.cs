using MDRDB.Recordsets;
using NPoco;

namespace MDRCloudServices.Services.Interfaces;

/// <summary>Storage database service</summary>
public interface IStorageDatabaseService
{
    /// <summary>Get database</summary>
    /// <param name="loc"></param>
    /// <returns></returns>
    IDatabase GetDatabase(Location loc);
}
