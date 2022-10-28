#if INTERNAL
using MDRCloudServices.Interfaces;
using MDRCloudServices.Services.Handlers;
using MediatR;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Handler for building OGC Location Cache</summary>
public class BuildOgcLocationCacheHandler : IRequestHandler<BuildOgcLocationCacheCommand>
{
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="m"></param>
    public BuildOgcLocationCacheHandler(IMediator m)
    {
        _m = m;
    }

    /// <summary>Handle building of OGC Location Cache</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Unit> Handle(BuildOgcLocationCacheCommand request, CancellationToken cancellationToken)
    {
        var user = await _m.Send(new GetUserPrincipleNameQuery(), cancellationToken);
        await _m.Send(new QueueFunctionCommand(
            "build-ogc-location-cache",
            $"{request.recordsetId},{user}"),
            cancellationToken);
        return Unit.Value;
    }
}
#endif
