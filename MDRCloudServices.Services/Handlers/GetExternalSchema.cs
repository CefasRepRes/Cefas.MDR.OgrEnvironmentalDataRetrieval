using MDRCloudServices.Services.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace MDRCloudServices.Services.Handlers;

public record GetExternalSchemaQuery() : IRequest<string>;

public class GetExternalSchemaHandler : IRequestHandler<GetExternalSchemaQuery, string>
{
    private readonly AppOptions _options;

    public GetExternalSchemaHandler(IOptions<AppOptions> options)
    {
        _options = options.Value;
    }

    public Task<string> Handle(GetExternalSchemaQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_options.ExternalSchema ?? string.Empty);
    }
}
