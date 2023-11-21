using System.Net;
using MDRCloudServices.DataLayer.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;

/// <summary>Query Builder for OGC Environmental Data Retrieval Queries</summary>
public static class OgcQueryBuilder
{
    /// <summary>Build date query</summary>
    /// <param name="builder">Existing query to expand</param>
    /// <param name="datetime">The date parameter</param>
    /// <param name="fieldname">The name of the field containing dates</param>
    public static void BuildDateQuery(Query builder, string? datetime, string? fieldname)
    {
        if (!string.IsNullOrEmpty(datetime) && !string.IsNullOrEmpty(fieldname))
        {
            if (datetime.Contains('/'))
            {
                BuildDateRangeQuery(builder, datetime, fieldname);
            }
            else
            {
                if (DateTime.TryParse(datetime, out var d))
                {
                    builder.WhereRaw($"ABS(DATEDIFF(day, {fieldname}, ?)) < 1", d);
                }
            }
        }
    }

    private static void BuildDateRangeQuery(Query builder, string datetime, string fieldname)
    {
        var dates = datetime.Split('/');
        if (dates.Length != 2) throw new ValidationException(new ErrorResponse
                {
                    { HttpStatusCode.BadRequest, "Number of date parameters is incorrect", nameof(datetime), null }
                });
        if (dates[0] != ".." && DateTime.TryParse(dates[0], out var startDate))
        {
            builder.Where(fieldname, ">=", startDate);
        }
        if (dates[1] != ".." && DateTime.TryParse(dates[1], out var endDate))
        {
            builder.Where(fieldname, "<=", endDate);
        }
    }

    /// <summary>Build bounding box query</summary>
    /// <param name="bbox">The sides of the bounding box</param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public static Polygon BuildBoundingBoxPolygon(double?[]? bbox)
    {
        if (bbox == null) throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
        double x1, y1, x2, y2;
        if (bbox.Length == 4)
        {
            x1 = bbox[0] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
            y1 = bbox[1] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
            x2 = bbox[2] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
            y2 = bbox[3] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
        }
        else if (bbox.Length == 6)
        {
            x1 = bbox[0] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
            y1 = bbox[1] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
            x2 = bbox[3] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
            y2 = bbox[4] ?? throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "BBOX Value missing", nameof(bbox), null } });
        }
        else throw new ValidationException(new ErrorResponse
        {
            { HttpStatusCode.BadRequest, "Number of bbox parameters is incorrect", nameof(bbox), null }
        });

        return new Polygon(new LinearRing(new Coordinate[]
        {
            new Coordinate(x1, y1),
            new Coordinate(x1, y2),
            new Coordinate(x2, y2),
            new Coordinate(x2, y1),
            new Coordinate(x1, y1),
        }));
    }

    /// <summary>Build overlapping geogemtry query</summary>
    /// <param name="builder"></param>
    /// <param name="geom"></param>
    public static void BuildGeometryQuery(Query builder, Geometry geom)
    {
        BuildGeometryQuery(builder, geom, "MDR_Geometry");
    }

    /// <summary>
    /// Build overlapping geogemtry query
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="geom"></param>
    /// <param name="column"></param>
    public static void BuildGeometryQuery(Query builder, Geometry geom, string column)
    {
        builder.WhereRaw($"\"{column}\".STIntersects(geometry::STGeomFromWKB(?, ?)) = 1", geom.AsBinary(), geom.SRID == -1 ? 4326 : geom.SRID);
    }

    /// <summary>Build geometry position query</summary>
    /// <param name="builder"></param>
    /// <param name="wkt"></param>
    public static void BuildPositionQuery(Query builder, string wkt)
    {
        BuildRadiusQuery(builder, wkt, 100, "m");
    }

    /// <summary>Supported Distance Units</summary>
    public static string[] DistanceUnits => new string[] {
        "nauticalmile", // 1852 metres
        "miles",
        "sheppey", // 7/8 of a mile (1.4 km), defined as the closest distance at which sheep remain picturesque.
        "km",
        "kilometres",
        "furlong", // 8 furlongs = 1 mile
        "chains", // 10 chains = 1 furlong
        "poles", // 5½ yards, 40 poles = 1 furlong
        "perch", // 5½ yards (alternative name of poles)
        "rod", // 5½ yards (alternative name of poles)
        "fathom", // 6 feet (1.8 metres)
        "smoot", // 170cm, defined as the height in 1958 of Oliver R. Smoot
        "m",
        "metres",
        "yard",
        "foot",
        "feet",
        "hand", // 4 inches
        "wiffle", // 89 millimeters, the diameter of a wiffle ball, a perforated, light-weight plastic ball frequently used by marine biologists as a size reference in photos to measure corals and other objects.
        "inch",
        "centimetre",
        "cm",
        "mm",
        "millimetre",
        "barleycorn" // ⅓ inch
    };

    /// <summary>Convert distance to meters</summary>
    /// <param name="distance"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public static double ConvertDistance(double distance, string units)
    {
        return units.ToLowerInvariant() switch
        {
            "nauticalmile" => distance * 1852,
            "miles" => distance * 1609.34,
            "sheppey" => distance * 1400,
            "km" => distance * 1000,
            "kilometres" => distance * 1000,
            "furlong" => distance * 201.1675,
            "chains" => distance * 20.11675,
            "poles" => distance * 5.029,
            "perch" => distance * 5.029,
            "rod" => distance * 5.029,
            "fathom" => distance * 1.8288,
            "smoot" => distance * 1.702,
            "m" => distance,
            "metres" => distance,
            "yard" => distance * 0.9144,
            "foot" => distance * 0.3048,
            "feet" => distance * 0.3048,
            "hand" => distance * 0.1016,
            "wiffle" => distance * 0.0889,
            "inch" => distance * 0.0254,
            "centimetre" => distance * 0.01,
            "cm" => distance * 0.01,
            "mm" => distance * 0.001,
            "millimetre" => distance * 0.001,
            "barleycorn" => distance * 0.00846667,
            _ => throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Invalid request: Unknown units", nameof(units), "ConvertDistance" } })
        };
    }

    /// <summary>Build geometry overlaps query with buffer around source</summary>
    /// <param name="builder">Existing query object</param>
    /// <param name="wkt">Well known text representation of the geometry</param>
    /// <param name="within">The buffer distance</param>
    /// <param name="withinUnits">The buffer distance units</param>
    public static void BuildRadiusQuery(Query builder, string wkt, double within, string withinUnits = "m")
    {
        var distance = ConvertDistance(within, withinUnits);

        try
        {
            var geom = new WKTReader().Read(wkt);
            // Geometry buffer uses degrees. Geography buffer allows units
            builder.WhereRaw(
                "MDR_Geometry.STIntersects(geometry::STGeomFromWKB((geography::STGeomFromWKB(?, ?).STBuffer(?)).STAsBinary(), 4326)) = 1",
                geom.AsBinary(),
                geom.SRID == -1 ? 4326 : geom.SRID,
                distance);
        }
        catch (ParseException ex)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Unable to parse Well Known Text", null, null } }, ex);
        }
    }

    /// <summary>Build overlaps query from geometry specified by well known text</summary>
    /// <param name="builder"></param>
    /// <param name="wkt"></param>
    public static void BuildAreaQuery(Query builder, string wkt)
    {
        try
        {
            var geometry = new WKTReader().Read(wkt);
            BuildGeometryQuery(builder, geometry);
        }
        catch (ParseException ex)
        {
            throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Unable to parse Well Known Text", null, null } }, ex);
        }
    }

    /// <summary>Build corridor query. Corridor width is total width so double the ammount to buffer</summary>
    /// <param name="builder"></param>
    /// <param name="wkt"></param>
    /// <param name="within"></param>
    /// <param name="withinUnits"></param>
    public static void BuildCorridorQuery(Query builder, string wkt, double within, string withinUnits = "m")
        => BuildRadiusQuery(builder, wkt, within / 2, withinUnits);
}
