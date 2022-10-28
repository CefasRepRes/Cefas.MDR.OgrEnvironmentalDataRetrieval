using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using MDRCloudServices.Api.Extensions;
using MDRCloudServices.Api.Filters;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.Helpers.Hyperlinkr;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MDRCloudServices.Api.Controllers.OGC;

/// <summary>Essential characteristics of the information available from the API</summary>
[ApiController]
[DisplayName("Capabilities")]
public class CapabilitiesController : Controller
{
    private readonly IMediator _m;

    public CapabilitiesController(IMediator m)
    {
        _m = m;
    }

    /// <summary>Landing page of this API</summary>
    /// <remarks>
    /// The landing page provides links to the API definition, 
    /// the Conformance statements and the metadata about the 
    /// feature data in this dataset.
    /// </remarks>
    /// <param name="query"></param>    
    /// <returns></returns>
    [HttpGet]
    [Route("ogc", Name = "OgcHome")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "Index")]
    [SwaggerResponse(StatusCodes.Status200OK, "Links to the API capabilities", typeof(string))]
    public async Task<IActionResult> IndexAsync([FromQuery] FormatQuery query)
    {
        try
        {
            var capabilities = await _m.Send(new CapabilitiesQuery());
            capabilities.SetLinks(Url, query);
            return capabilities.SerialiseFormat(this, query.f ?? "json");
        }
        catch (ValidationException ex)
        {
            return ex.Result.ToActionResult();
        }
    }

    /// <summary>Information about stardards to which this API conforms</summary>
    /// <param name="f">Format to return the data response in</param>
    /// <returns></returns>
    [HttpGet]
    [Route("ogc/conformance", Name = "OgcConformance")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "Conformance")]
    [SwaggerResponse(StatusCodes.Status200OK, "The URIs of all requirements classes supported by the server", typeof(string))]
    public IActionResult Conformance([FromQuery] string f = "json")
    {
        var conformance = new Conformance
        {
            ConformsTo = new()
            {
                "http://www.opengis.net/spec/ogcapi-edr-1/1.0/conf/core",
                "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/core",
                "http://www.opengis.net/spec/ogcapi-common-2/1.0/conf/collections",
                "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/oas3",
                "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/html",
                "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/json",
            }
        };
        return conformance.SerialiseFormat(this, f);
    }

    /// <summary>Provide a list of available data groups</summary>
    /// <param name="f">Format to return the data response in</param>
    /// <returns></returns>
    [HttpGet]
    [Route("ogc/groups", Name = "OgcGroups")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "Groups")]
    [SwaggerResponse(StatusCodes.Status200OK, "List the available data groups.", typeof(string[]))]
    public IActionResult Groups([FromQuery] string f = "json")
    {
        try
        {
            var model = new OgcMembers();
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.Groups(f)).ToString(),
                Hreflang = "en",
                Rel = "self",
                Type = f switch
                {
                    "json" => "application/json",
                    "yaml" => "application/yaml",
                    "xml" => "application/xml",
                    "html" => "text/html",
                    _ => throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Bad format", "", "" } })
                }
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.Groups("json")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/json"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.Groups("xml")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/xml"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.Groups("yaml")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/yaml"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.Groups("html")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "text/html"
            });
            return model.SerialiseFormat(this, f);
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
    }

    /// <summary>List of links to information available in the group</summary>
    /// <param name="id">Identifier (name) of a specific group</param>
    /// <param name="f">Format to return the data response in</param>
    /// <returns></returns>
    [HttpGet]
    [Route("ogc/groups/{id}", Name = "OgcGroupsId")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "GroupsId")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of links to the data members of the group, these can either be other groups or collections.", typeof(string))]
    public IActionResult GroupsId([FromRoute] string id, [FromQuery] string f = "json")
    {
        try
        {
            var model = new OgcMembers();
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.GroupsId(id, f)).ToString(),
                Hreflang = "en",
                Rel = "self",
                Type = f switch
                {
                    "json" => "application/json",
                    "yaml" => "application/yaml",
                    "xml" => "application/xml",
                    "html" => "text/html",
                    _ => throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Bad format", "", "" } })
                }
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.GroupsId(id, "json")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/json"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.GroupsId(id, "xml")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/xml"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.GroupsId(id, "yaml")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/yaml"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.GroupsId(id, "html")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "text/html"
            });
            return model.SerialiseFormat(this, f);
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
    }

    /// <summary>List the available collections from the service</summary>
    /// <param name="bbox">
    /// Only features that have a geometry that intersects the bounding box are selected. 
    /// The bounding box is provided as four or six numbers, depending on whether the 
    /// coordinate reference system includes a vertical axis (height or depth):
    /// <ul>
    /// <li>Lower left corner, coordinate axis 1</li>
    /// <li>Lower left corner, coordinate axis 2</li>
    /// <li>Minimum value, coordinate axis 3 (optional)</li>
    /// <li>Upper right corner, coordinate axis 1</li>
    /// <li>Upper right corner, coordinate axis 2</li>
    /// <li>Maximum value, coordinate axis 3 (optional)</li>
    /// </ul>
    /// The coordinate reference system of the values is WGS 84 longitude/latitude 
    /// (http://www.opengis.net/def/crs/OGC/1.3/CRS84) unless a different coordinate 
    /// reference system is specified in the parameter <code>bbox-crs</code>. For WGS 84 
    /// longitude/latitude the values are in most cases the sequence of minimum longitude,
    /// minimum latitude, maximum longitude and maximum latitude. However, in cases where 
    /// the box spans the antimeridian the first value (west-most box edge) is larger 
    /// than the third value (east-most box edge). If the vertical axis is included, 
    /// the third and the sixth number are the bottom and the top of the 3-dimensional 
    /// bounding box. If a feature has multiple spatial geometry properties, it is the 
    /// decision of the server whether only a single spatial geometry property is used 
    /// to determine the extent or all relevant geometries.
    /// </param>
    /// <param name="datetime">
    /// Either a date-time or an interval, open or closed. Date and time expressions 
    /// adhere to RFC 3339. Open intervals are expressed using double-dots. Examples:
    /// <ul>
    /// <li>A date-time: "2018-02-12T23:20:50Z"</li>
    /// <li>A closed interval: "2018-02-12T00:00:00Z/2018-03-18T12:31:12Z"</li>
    /// <li>Open intervals: "2018-02-12T00:00:00Z/.." or "../2018-03-18T12:31:12Z"</li>
    /// </ul>
    /// Only features that have a temporal property that intersects the value of datetime 
    /// are selected. If a feature has multiple temporal properties, it is the decision 
    /// of the server whether only a single temporal property is used to determine the 
    /// extent or all relevant temporal properties.
    /// </param>
    /// <param name="f">Format to return the data response in</param>
    /// <returns></returns>
    [HttpGet]
    [Route("ogc/collections", Name = "OgcCollections")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "Collections")]
    [SwaggerResponse(StatusCodes.Status200OK, "Metdata about the Environmental data collections shared by this API", typeof(string))]
    public async Task<IActionResult> CollectionsAsync([FromQuery] double?[]? bbox, [FromQuery] string? datetime, [FromQuery] string f = "json")
    {
        try
        {
            var model = await _m.Send(new CollectionsQuery(bbox, datetime));

            foreach (var collection in model.Collections)
            {
                await collection.SetLinksAsync(Url, _m, f);
            }

            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.CollectionsAsync(bbox, datetime, f)).ToString(),
                Hreflang = "en",
                Rel = "self",
                Type = f switch
                {
                    "json" => "application/json",
                    "yaml" => "application/yaml",
                    "xml" => "application/xml",
                    "html" => "text/html",
                    _ => throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Bad format", "", "" } })
                }
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.CollectionsAsync(bbox, datetime, "json")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/json"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.CollectionsAsync(bbox, datetime, "xml")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/xml"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.CollectionsAsync(bbox, datetime, "yaml")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "application/yaml"
            });
            model.Links.Add(new()
            {
                Href = Url.GetLink<CapabilitiesController, IActionResult>(x => x.CollectionsAsync(bbox, datetime, "html")).ToString(),
                Hreflang = "en",
                Rel = "alternative",
                Type = "text/html"
            });
            return model.SerialiseFormat(this, f);
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
    }
}
