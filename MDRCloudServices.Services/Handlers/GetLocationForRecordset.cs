using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;

namespace MDRCloudServices.Services.Handlers;

public record GetLocationForRecordsetQuery(Recordset rs): IRequest<Location>;

public class GetLocationForRecordsetHandler : IRequestHandler<GetLocationForRecordsetQuery, Location>
{
    private readonly IRecordsetService _recordsetService;

    public GetLocationForRecordsetHandler(IRecordsetService recordsetService)
    {
        _recordsetService = recordsetService;
    }

    public async Task<Location> Handle(GetLocationForRecordsetQuery request, CancellationToken cancellationToken)
    {
        return await _recordsetService.GetLocationForRecordsetAsync(request.rs);
    }
}
