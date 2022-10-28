using System.ComponentModel;
using System.Data.SqlClient;
using System.Net;
using MDRCloudServices.Api.Filters;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.Exceptions;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlKata;

namespace MDRCloudServices.Api.Controllers.OGC;

/// <summary>Data queries available</summary>
[DisplayName("Collection data queries")]
[ApiController]
[Route("ogc/collections")]
public class CollectionDataQueryController : ControllerBase
{
    private readonly IMediator _m;
    public CollectionDataQueryController(IMediator m)
    {
        _m = m;
    }

    /// <summary>Query end point for position queries of collection {collectionId}</summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="coords">
    /// Location(s) to return data for, the coordinates are defined by a Well Known Text (wkt) string. 
    /// To retrieve a single location:
    ///
    /// POINT(x y) i.e. POINT(0 51.48) for Greenwich, London
    /// 
    /// And for a list of locations
    /// 
    /// MULTIPOINT((x y),(x1 y1),(x2 y2),(x3 y3))
    /// 
    /// i.e. MULTIPOINT((38.9 -77),(48.85 2.35),(39.92 116.38),(-35.29 149.1),(51.5 -0.1))
    /// 
    /// see http://portal.opengeospatial.org/files/?artifact_id=25355 and 
    /// https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry
    /// 
    /// The coordinate values will depend on the CRS parameter, if this is not defined 
    /// the values will be assumed to WGS84 values (i.e x=longitude and y=latitude)
    /// </param>
    /// <param name="spatialQuery"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Position")]
    [Route("{collectionId}/position", Name = "OgcCollectionDataQueryPosition")]
    public async Task<IActionResult> PositionAsync(
        [FromRoute] string collectionId,
        [FromQuery] string coords,
        [FromQuery] SpatialQuery spatialQuery,
        CancellationToken ct) => await PositionQueryAsync(collectionId, coords, spatialQuery, ct);

    /// <summary>Used by multiple endpoints</summary>
    /// <param name="collectionId"></param>
    /// <param name="coords"></param>
    /// <param name="spatialQuery"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<IActionResult> PositionQueryAsync(
        string collectionId,
        string coords,
        SpatialQuery spatialQuery,
        CancellationToken ct)
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "PositionAsync" } });
            }

            return File(await _m.Send(new DataQuery(
                    id,
                    null,
                    (Query q) => OgcQueryBuilder.BuildPositionQuery(q, coords),
                    spatialQuery.datetime,
                    spatialQuery.parameterName ?? string.Empty,
                    spatialQuery.crs ?? "native",
                    spatialQuery.f ?? "geoJson"), ct), "application/geo+json");
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("geography"))
            {
                return BadRequest("Unable to parse Well Known Text");
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Query end point for radius queries of collection {collectionId}
    /// </summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="spatialQuery"></param>
    /// <param name="coords">	
    /// Location(s) to return data for, the coordinates are defined by a Well Known Text (WKT) string. 
    /// To retrieve a single location:
    ///
    /// POINT(x y) i.e.POINT(0 51.48) for Greenwich, London
    ///
    /// See http://portal.opengeospatial.org/files/?artifact_id=25355 and 
    /// https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry
    ///
    /// The coordinate values will depend on the CRS parameter, if this is not defined the 
    /// values will be assumed to WGS84 values(i.e x = longitude and y = latitude)
    /// </param>
    /// <param name="within">Defines radius of area around defined coordinates to include in the data selection</param>
    /// <param name="withinUnits">Distance units for the within parameter</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Radius")]
    [Route("{collectionId}/radius", Name = "OgcCollectionDataQueryRadius")]
    public async Task<IActionResult> RadiusAsync(
    [FromRoute] string collectionId,
    [FromQuery] SpatialQuery spatialQuery,
    [FromQuery] string coords,
    [FromQuery] double within,
    [FromQuery(Name = "within-units")] string withinUnits,
    CancellationToken ct)
    {
        if (string.IsNullOrEmpty(withinUnits)) withinUnits = "m";
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "RadiusAsync" } });
            }
            return File(await _m.Send(new DataQuery(
                    id,
                    null,
                    (Query q) => OgcQueryBuilder.BuildRadiusQuery(q, coords, within, withinUnits),
                    spatialQuery.datetime,
                    spatialQuery.parameterName,
                    spatialQuery.crs ?? "native",
                    spatialQuery.f ?? "geojson"), ct), "application/geo+json");
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("geography"))
            {
                return BadRequest("Unable to parse Well Known Text");
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>Query end point for area queries of collection {collectionId} defined by a polygon</summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="coords">	  
    /// Only data that has a geometry that intersects the area defined by the polygon are selected.
    ///
    /// The polygon is defined using a Well Known Text string following
    ///
    /// coords=POLYGON((x y, x1 y1, x2 y2,..., xn yn x y))
    ///
    /// which are values in the coordinate system defined by the crs query parameter (if crs is not defined 
    /// the values will be assumed to be WGS84 longitude/latitude coordinates).
    ///
    /// For instance a polygon that roughly describes an area that contains South West England in WGS84 would look like:
    ///
    /// coords=POLYGON((-6.1 50.3,-4.35 51.4,-2.6 51.6,-2.8 50.6,-5.3 49.9,-6.1 50.3))
    ///
    /// See http://portal.opengeospatial.org/files/?artifact_id=25355 and 
    /// https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry
    ///
    /// The coords parameter will only support 2D POLYGON definitions</param>
    /// <param name="spatialQuery"></param>
    /// <param name="resolutionX">
    /// Defined it the user requires data at a different resolution from the native resolution of the data along the x-axis
    ///
    /// If this is a single value it denotes the number of intervals to retrieve data for along the x-axis
    ///
    /// i.e.resolution-x= 10
    ///
    /// would retrieve 10 values along the x-axis from the minimum x coordinate to maximum x coordinate 
    /// (i.e. a value at both the minimum x and maximum x coordinates and 8 values between).
    /// </param>
    /// <param name="resolutionY">
    /// 
    /// </param>
    /// <param name="ct"></param>
    /// <returns>
    /// Defined it the user requires data at a different resolution from the native resolution of the data along the y-axis
    ///
    /// If this is a single value it denotes the number of intervals to retrieve data for along the y-axis
    /// 
    /// i.e.resolution-y= 10
    /// 
    /// would retrieve 10 values along the y-axis from the minimum y coordinate to maximum y coordinate 
    /// (i.e. a value at both the minimum y and maximum y coordinates and 8 values between).
    /// </returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Area")]
    [Route("{collectionId}/area", Name = "OgcCollectionDataQueryArea")]
    public async Task<IActionResult> AreaAsync(
        [FromRoute] string collectionId,
        [FromQuery] string coords,
        [FromQuery] SpatialQuery spatialQuery,
        [FromQuery(Name = "resolution-x")] int? resolutionX,
        [FromQuery(Name = "resolution-y")] int? resolutionY,
        CancellationToken ct)
    {
        try
        {
            if (resolutionX != null || resolutionY != null)
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Resoultion paramters not supported", "", "" } });
            }
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "AreaAsync" } });
            }
            return File(await _m.Send(new DataQuery(
                    id,
                    null,
                    (Query q) => OgcQueryBuilder.BuildAreaQuery(q, coords),
                    spatialQuery.datetime,
                    spatialQuery.parameterName,
                    spatialQuery.crs ?? "native",
                    spatialQuery.f ?? "geojson"), ct), "application/geo+json");
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("geography"))
            {
                return BadRequest("Unable to parse Well Known Text");
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>Query end point for Cube queries of collection {collectionId} defined by a cube</summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Cube")]
    [Route("{collectionId}/cube", Name = "OgcCollectionDataQueryCube")]
    public async Task<IActionResult> CubeAsync(
        [FromRoute] string collectionId,
        [FromQuery] RequiredSpatialBboxQuery query,
        CancellationToken ct)
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "AreaAsync" } });
            }
            var polygon = OgcQueryBuilder.BuildBoundingBoxPolygon(query.bbox);
            return File(await _m.Send(new DataQuery(
                    id,
                    null,
                    (Query q) => OgcQueryBuilder.BuildGeometryQuery(q, polygon),
                    query.datetime,
                    query.parameterName,
                    query.crs ?? "native",
                    query.f ?? "geojson"), ct), "application/geo+json");            
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("geography"))
            {
                return BadRequest("Unable to parse Well Known Text");
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Query end point for trajectory queries of collection {collectionId} defined
    /// by a wkt linestring and a iso8601 time period
    /// </summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="coords">
    /// Only data that has a geometry that intersects the area defined by the linestring are selected.
    ///
    /// The trajectory is defined using a Well Known Text string following
    ///
    /// A 2D trajectory, on the surface of earth with no time or height dimensions: 
    /// coords=LINESTRING(-2.87 51.14 , -2.98 51.36,-3.15 51.03 ,-3.48 50.74 ,-3.36 50.9 )
    ///
    /// A 2D trajectory, on the surface of earth with all for the same time and no height dimension, 
    /// time value defined in ISO8601 format by the datetime query parameter: 
    /// coords=LINESTRING(-2.87 51.14 , -2.98 51.36 ,-3.15 51.03 ,-3.48 50.74 ,-3.36 50.9 )&amp;time=2018-02-12T23:00:00Z
    ///
    /// A 2D trajectory, on the surface of earth with no time value but at a fixed height level, 
    /// height defined in the collection height units by the z query parameter: 
    /// coords=LINESTRING(-2.87 51.14, -2.98 51.36, -3.15 51.03, -3.48 50.74, -3.36 50.9)&amp;z=850
    ///
    /// A 2D trajectory, on the surface of earth with all for the same time and at a fixed height level, 
    /// time value defined in ISO8601 format by the datetime query parameter and height defined in the 
    /// collection height units by the z query parameter: 
    /// coords=LINESTRING(-2.87 51.14, -2.98 51.36, -3.15 51.03, -3.48 50.74, -3.36 50.9)&amp;time=2018-02-12T23:00:00Z&amp;z=850
    /// </param>
    /// <param name="spatialQuery"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Trajectory")]
    [Route("{collectionId}/trajectory", Name = "OgcCollectionDataQueryTrajectory")]
    public async Task<IActionResult> TrajectoryAsync(
        [FromRoute] string collectionId,
        [FromQuery] string coords,
        [FromQuery] SpatialQuery spatialQuery,
        CancellationToken ct) => await PositionQueryAsync(collectionId, coords, spatialQuery, ct);

    /// <summary>Query end point for Corridor queries of collection {collectionId} defined by a polygon</summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="coords">
    /// Only data that has a geometry that intersects the area defined by the linestring are selected.
    /// 
    /// The trajectory is defined using a Well Known Text string following
    /// 
    /// A 2D trajectory, on the surface of earth with no time or height dimensions: 
    /// coords=LINESTRING(-2.87 51.14, -2.98 51.36,-3.15 51.03, -3.48 50.74, -3.36 50.9)
    /// 
    /// A 2D trajectory, on the surface of earth with all for the same time and no height 
    /// dimension, time value defined in ISO8601 format by the datetime query parameter: 
    /// coords=LINESTRING(-2.87 51.14 , -2.98 51.36 ,-3.15 51.03 ,-3.48 50.74 ,-3.36 50.9 )&amp;time=2018-02-12T23:00:00Z
    /// 
    /// A 2D trajectory, on the surface of earth with no time value but at a fixed height 
    /// level, height defined in the collection height units by the z query parameter: 
    /// coords=LINESTRING(-2.87 51.14, -2.98 51.36, -3.15 51.03, -3.48 50.74, -3.36 50.9)&amp;z=850
    /// 
    /// A 2D trajectory, on the surface of earth with all for the same time and at a fixed 
    /// height level, time value defined in ISO8601 format by the datetime query parameter 
    /// and height defined in the collection height units by the z query parameter: 
    /// coords=LINESTRING(-2.87 51.14, -2.98 51.36, -3.15 51.03, -3.48 50.74, -3.36 50.9)&amp;time=2018-02-12T23:00:00Z&amp;z=850
    /// </param>
    /// <param name="spatialQuery"></param>
    /// <param name="corridorQuery"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Corridor")]
    [Route("{collectionId}/corridor", Name = "OgcCollectionDataQueryCorridor")]
    public async Task<IActionResult> CorridorAsync(
        [FromRoute] string collectionId,
        [FromQuery] string coords,
        [FromQuery] SpatialQuery spatialQuery,
        [FromQuery] CorridorQuery corridorQuery,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!int.TryParse(collectionId, out int id))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "CorridorAsync" } });
            }
            return File(await _m.Send(new DataQuery(
                    id,
                    null,
                    (Query q) => OgcQueryBuilder.BuildCorridorQuery(q, coords, corridorQuery.width, corridorQuery.widthUnits),
                    spatialQuery.datetime,
                    spatialQuery.parameterName,
                    spatialQuery.crs ?? "native",
                    spatialQuery.f ?? "geojson"), cancellationToken), "application/geo+json");
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("geography"))
            {
                return BadRequest("Unable to parse Well Known Text");
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>Query end point to retrieve data from collection {collectionId} using a unique identifier</summary>
    /// <param name="collectionId">Identifier (id) of a specific collection</param>
    /// <param name="itemId">Retrieve data from the collection using a unique identifier.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [LogRequest(ObjectType = "OgcCollectionDataQuery", ObjectVerb = "Item")]
    [Route("{collectionId}/items/{itemId}", Name = "OgcCollectionDataQueryItem")]
    public async Task<IActionResult> ItemAsync(
        [FromRoute] string collectionId,
        [FromRoute] string itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!int.TryParse(collectionId, out int cid))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Collection not found", "collecitonId", "ItemAsync" } });
            }
            if (!int.TryParse(itemId, out int iid))
            {
                throw new ValidationException(new ErrorResponse { { HttpStatusCode.NotFound, "Item not found", "itemId", "ItemAsync" } });
            }

            return File(await _m.Send(new ItemFromCollectionQuery(cid, iid), cancellationToken), "application/geo+json");
        }
        catch (ValidationException vex)
        {
            return vex.Result.ToActionResult();
        }
        catch (NotFoundException nfex)
        {
            return NotFound(nfex.Message);
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("geography"))
            {
                return BadRequest("Unable to parse Well Known Text");
            }
            else
            {
                throw;
            }
        }
    }
}
