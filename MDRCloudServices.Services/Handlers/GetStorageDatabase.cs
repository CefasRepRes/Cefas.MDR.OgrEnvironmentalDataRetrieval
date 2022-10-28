using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;
using NPoco;

namespace MDRCloudServices.Services.Handlers;

public record GetStorageDatabaseQuery(Location location): IRequest<IDatabase>;

public class GetStorageDatabaseHandler : IRequestHandler<GetStorageDatabaseQuery, IDatabase>
{
    private readonly IStorageDatabaseService _storageDatabaseService;

    public GetStorageDatabaseHandler(IStorageDatabaseService storageDatabaseService)
    {
        _storageDatabaseService = storageDatabaseService;
    }

    public Task<IDatabase> Handle(GetStorageDatabaseQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_storageDatabaseService.GetDatabase(request.location));
    }
}
