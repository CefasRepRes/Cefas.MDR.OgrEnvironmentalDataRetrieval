using System.Net.Mime;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MDRCloudServices.Api.Filters;

public class AssignContentTypeFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses.ContainsKey("200"))
        {
            operation.Responses.Clear();
        }

        var data = new OpenApiResponse
        {
            Description = "Ok",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [MediaTypeNames.Application.Json] = new OpenApiMediaType(),
                [MediaTypeNames.Application.Xml] = new OpenApiMediaType()
            }
        };

        operation.Responses.Add("200", data);
    }
}
