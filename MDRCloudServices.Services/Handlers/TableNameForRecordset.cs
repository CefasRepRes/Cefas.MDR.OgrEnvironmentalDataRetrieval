using System.Threading;
using System.Threading.Tasks;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;

namespace MDRCloudServices.Services.Handlers;

public record TableNameForRecordsetQuery(Recordset rs, Location? location) : IRequest<string>;

public class TableNameForRecordsetHandler : IRequestHandler<TableNameForRecordsetQuery, string>
{
    private readonly IRecordsetTableService service;

    public TableNameForRecordsetHandler(IRecordsetTableService recordsetService)
    {
        service = recordsetService;
    }

    public async Task<string> Handle(TableNameForRecordsetQuery request, CancellationToken cancellationToken)
    {
        if (request.location is null)
        {
            return await service.TableNameForRecordsetAsync(request.rs);
        }
        else
        {
            return service.TableNameForRecordset(request.rs, request.location);
        }
    }
}
