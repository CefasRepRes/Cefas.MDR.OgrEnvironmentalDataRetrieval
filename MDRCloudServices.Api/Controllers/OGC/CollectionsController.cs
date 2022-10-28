using System.ComponentModel;
using System.Net;
using MDRCloudServices.Api.Extensions;
using MDRCloudServices.Api.Filters;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlKata;
using Swashbuckle.AspNetCore.Annotations;

namespace MDRCloudServices.Api.Controllers.OGC;

/// <summary>Description of the information available from the collections</summary>
[DisplayName("Collection metadata")]
[ApiController]
public class CollectionsController : Controller
{
    private readonly IMediator _m;

    /// <summary>Default constructor</summary>
    /// <param name="m"></param>
    public CollectionsController(IMediator m)
    {
        _m = m;
    }

    /// <summary>List query types supported by the collection</summary>
    /// <remarks>
    /// This will provide information about the query types that are 
    /// supported by the chosen collection Use content negotiation 
    /// to request HTML or JSON.
    /// </remarks>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="f">Format to return the data response in</param>
    /// <returns></returns>
    [HttpGet]
    [Route("ogc/collections/{collectionId}", Name = "OgcCollectionId")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "CollectionsId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Metadata about the {collectionId} collection shared by this API.", typeof(string))]
    public async Task<IActionResult> CollectionItemAsync([FromRoute] string collectionId, [FromQuery] string? f)
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "InstancesAsync" } });
            }
            OgcCollection model = await _m.Send(new CollectionByIdQuery(id, null, null));

            await model.SetLinksAsync(Url, _m, f ?? "json");

            return model.SerialiseFormat(this, f ?? "json");
        }
        catch (ValidationException ex)
        {
            return ex.Result.ToActionResult();
        }

        throw new NotImplementedException();
    }

    /// <summary>List available items</summary>
    /// <remarks>List the items available in the collection accessible via a unique identifier</remarks>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
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
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    [Route("ogc/collections/{collectionId}/items", Name = "OgcCollectionIdItems")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "CollectionsIdItems")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of pre-existing items available for retrieval", typeof(string))]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Data request still being processed", typeof(string))]
    [SwaggerResponse(StatusCodes.Status308PermanentRedirect, "Request will take a significant time to process", typeof(string))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Requested data not found", typeof(string))]
    [SwaggerResponse(StatusCodes.Status413PayloadTooLarge, "Requested data volume to large to be handled by this service", typeof(string))]
    public async Task<IActionResult> ItemsAsync(
        [FromRoute] string collectionId, 
        [FromQuery] double?[]? bbox, 
        [FromQuery] string? datetime,
        CancellationToken ct)
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "InstancesAsync" } });
            }
            var polygon = OgcQueryBuilder.BuildBoundingBoxPolygon(bbox);
            var model = await _m.Send(new ItemsInCollectionQuery(
                id,
                null,
                (Query q) => OgcQueryBuilder.BuildGeometryQuery(q, polygon),
                datetime), ct);

            return Json(model);
        }
        catch (ValidationException ex)
        {
            return ex.Result.ToActionResult();
        }
    }

    /// <summary>List data instances of {collectionId}</summary>
    /// <remarks>This will provide list of the available instances of the collection. Use content negotiation to request HTML or JSON.</remarks>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="f">Format to return the data response in</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    [Route("ogc/collections/{collectionId}/instances", Name = "OgcCollectionIdInstances")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "CollectionsIdInstances")]
    [SwaggerResponse(StatusCodes.Status200OK, "Metadata about the instance of {collectionId} collection shared by this API", typeof(string))]
    public async Task<IActionResult> InstancesAsync([FromRoute] string collectionId, [FromQuery] string f = "json")
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "InstancesAsync" } });
            }
            var model = await _m.Send(new InstancesForCollectionQuery(id));
            return model.SerialiseFormat(this, f);
        }
        catch (ValidationException ex)
        {
            return ex.Result.ToActionResult();
        }
    }

    /// <summary>List available location identifers for the instance</summary>
    /// <remarks>List the locations available for the collection</remarks>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
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
    [Route("ogc/collections/{collectionId}/locations", Name = "OgcCollectionIdLocations")]
    [LogRequest(ObjectType = "Ogc", ObjectVerb = "CollectionsIdLocations")]
    [SwaggerResponse(StatusCodes.Status200OK, "Metadata about the instance of {collectionId} collection shared by this API", typeof(string))]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Data request still being processed", typeof(string))]
    [SwaggerResponse(StatusCodes.Status308PermanentRedirect, "Request will take a significant time to process", typeof(string))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Requested data not found", typeof(string))]
    [SwaggerResponse(StatusCodes.Status413PayloadTooLarge, "Requested data volume to large to be handled by this service", typeof(string))]
    public async Task<IActionResult> LocationsAsync([FromRoute] string collectionId, [FromQuery] double?[]? bbox, [FromQuery] string? datetime)
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "CorridorAsync" } });
            }
            return Ok(await _m.Send(new LocationsForCollectionQuery(id, null, bbox, datetime)));
        }
        catch (ValidationException ex)
        {
            return ex.Result.ToActionResult();
        }
    }
}
