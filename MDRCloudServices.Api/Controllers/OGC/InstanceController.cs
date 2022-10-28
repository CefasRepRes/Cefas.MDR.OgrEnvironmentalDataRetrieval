using System.ComponentModel;
using System.Net;
using MDRCloudServices.Api.Filters;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MDRCloudServices.Api.Controllers.OGC;

/// <summary>Description of the information available from instances of a collection</summary>
[DisplayName("Instance metadata")]
[ApiController]
public class InstanceController : ControllerBase
{
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="m"></param>
    public InstanceController(IMediator m)
    {
        _m = m;
    }

    /// <summary>List available location identifers for the instance</summary>
    /// <remarks>List the locations available for the instance of the collection</remarks>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="instanceId">Identifier (id) of a specific instance of a collection</param>
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
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    [Route("ogc/collections/{collectionId}/instances/{instanceId}/locations", Name = "OgcInstanceLocations")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "InstanceLocations")]
    [SwaggerResponse(StatusCodes.Status200OK, "Metadata about the instance of {collectionId} collection shared by this API", typeof(string))]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Data request still being processed", typeof(string))]
    [SwaggerResponse(StatusCodes.Status308PermanentRedirect, "Request will take a significant time to process", typeof(string))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Requested data not found", typeof(string))]
    [SwaggerResponse(StatusCodes.Status413PayloadTooLarge, "Requested data volume to large to be handled by this service", typeof(string))]
    public async Task<IActionResult> InstancesAsync(
        [FromRoute] string collectionId, 
        [FromRoute] string instanceId, 
        [FromQuery] double?[]? bbox, 
        [FromQuery] string? datetime)
    {
        try
        {
            if (!int.TryParse(collectionId, out var cid))
            {
                throw new ValidationException(new ErrorResponse() { { HttpStatusCode.NotFound, "Colleciton not found", "collectionId", null } });
            }
            if (!int.TryParse(instanceId, out var iid))
            {
                throw new ValidationException(new ErrorResponse() { { HttpStatusCode.NotFound, "Instance not found", "instanceId", null } });
            }
            var results = await _m.Send(new LocationsForCollectionQuery(cid, iid, bbox, datetime));
            return Ok(results);
        }
        catch (ValidationException ex)
        {
            return ex.Result.ToActionResult();
        }
    }
}
