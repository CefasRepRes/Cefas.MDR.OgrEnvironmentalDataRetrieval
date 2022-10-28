using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MDRCloudServices.Api.Models;

/// <summary>Convention to separate OGC controllers into a separate Swagger document</summary>
public class ApiExplorerGroupByNamespace : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace ?? string.Empty;
        controller.ApiExplorer.GroupName = controllerNamespace.EndsWith("OGC") ? "ogc" : "v2";

        foreach (var attribute in controller.Attributes)
        {
            if (attribute.GetType() == typeof(DisplayNameAttribute))
            {
                var attrib = (DisplayNameAttribute)attribute;
                if (!string.IsNullOrWhiteSpace(attrib.DisplayName))
                    controller.ControllerName = attrib.DisplayName;
            }
        }
    }
}
