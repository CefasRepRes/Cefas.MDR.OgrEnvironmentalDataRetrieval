using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;

/// <summary>Format query</summary>
public class FormatQuery
{
    /// <summary>Format to return the data response in</summary>
    [FromQuery] public string? f { get; set; }
}

/// <summary>Format date time query</summary>
public class FormatDateTimeQuery : FormatQuery
{
    /// <summary>
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
    /// </summary>
    [FromQuery] public string? datetime { get; set; }
}

/// <summary>Format date time crs query</summary>
public class FormatDateTimeCrsQuery : FormatDateTimeQuery
{
    /// <summary>
    /// identifier (id) of the coordinate system to return data in list of valid crs identifiers for the chosen collection are defined in the metadata responses. If not supplied the coordinate reference system will default to WGS84.
    /// </summary>
    [FromQuery] public string? crs { get; set; }
}

/// <summary>Spatial query</summary>
public class SpatialQuery : FormatDateTimeCrsQuery
{
    /// <summary>
    /// Define the vertical level to return data from i.e. z=level
    /// for instance if the 850hPa pressure level is being queried
    ///
    /// z=850
    ///
    /// or a range to return data for all levels between and including 2 defined levels i.e.z=minimum value/maximum value
    ///
    /// for instance if all values between and including 10m and 100m
    ///
    /// z=10/100
    ///
    /// finally a list of height values can be specified i.e.z=value1,value2,value3
    ///
    /// for instance if values at 2m, 10m and 80m are required
    ///
    /// z=2,10,80
    ///
    /// An Arithmetic sequence using Recurring height intervals, the difference is the number of recurrences is defined at the start and the amount to increment the height by is defined at the end
    ///
    /// i.e.z=Rn/min height/height interval
    ///
    /// so if the request was for 20 height levels 50m apart starting at 100m:
    ///
    /// z=R20/100/50
    /// 
    /// When not specified data from all available heights SHOULD be returned
    /// </summary>
    [FromQuery] public string? z { get; set; }

    /// <summary>
    /// comma delimited list of parameters to retrieve data for. Valid parameters are listed in the collections metadata
    /// </summary>
    [FromQuery(Name = "parameter-name")] public string? parameterName { get; set; }
}

/// <summary>Spatial query with bounding box</summary>
public class SpatialBboxQuery : SpatialQuery
{
    /// <summary>
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
    /// </summary>
    [FromQuery] public double?[]? bbox { get; set; }
}

/// <summary>Spatial query with bounding box</summary>
public class RequiredSpatialBboxQuery : SpatialQuery
{
    /// <summary>
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
    /// </summary>
    [FromQuery] [Required] public double?[]? bbox { get; set; }
}

/// <summary>Parameters for corridor queries</summary>
public class CorridorQuery
{
    /// <summary>
    /// Width of the corridor
    ///
    /// The width value represents the whole width of the corridor where the trajectory supplied 
    /// in the coords query parameter is the centre point of the corridor.
    ///
    /// corridor-width={width}
    ///
    /// e.g.corridor-width=100
    ///
    /// Would be a request for a corridor 100 units wide with the coords parameter values being 
    /// the centre point of the requested corridor, the request would be for data values 50 units 
    /// either side of the trajectory coordinates defined in the coords parameter.
    ///
    /// The width units supported by the collection will be provided in the API metadata responses
    /// </summary>
    [FromQuery(Name = "corridor-width")] public double width { get; set; } = 1;

    /// <summary>Distance units for the corridor-width parameter. e.g. width-units=KM</summary>
    [FromQuery(Name = "width-units")] public string widthUnits { get; set; } = "m";

    /// <summary>
    /// Height of the corridor
    /// 
    /// The height value represents the whole height of the corridor where the trajectory 
    /// supplied in the coords query parameter is the centre point of the corridor
    ///
    /// corridor-height={height}
    ///
    /// e.g. corridor-height=100
    ///
    /// Would be a request for a corridor 100 units high with the coords parameter values being the 
    /// centre point of the requested corridor, the request would be for data values 50 units either 
    /// side of the trajectory coordinates defined in the coords parameter.
    ///
    /// The height units supported by the collection will be provided in the API metadata responses
    /// </summary>
    [FromQuery(Name = "corridor-height")] public double height { get; set; } = 6371; // Radius of the earth

    /// <summary>Distance units for the corridor-height parameter. e.g. height-units=KM</summary>
    [FromQuery(Name = "height-units")] public string heightUnits { get; set; } = "km";

    /// <summary>
    /// Defined it the user requires data at a different resolution from the native resolution of the data along the x-axis
    ///
    /// If this is a single value it denotes the number of intervals to retrieve data for along the x-axis
    ///
    /// i.e. resolution-x=10
    ///
    /// would retrieve 10 values along the x-axis from the minimum x coordinate to maximum 
    /// x coordinate (i.e. a value at both the minimum x and maximum x coordinates and 8 values between).
    /// </summary>
    [FromQuery(Name ="resolution-x")] public double? resX { get; set; }

    /// <summary>
    /// Defined it the user requires data at a different resolution from the native resolution of the data along the z-axis
    ///
    /// If this is a single value it denotes the number of intervals to retrieve data for along the z-axis
    ///
    /// i.e. resolution-z=10
    ///
    /// would retrieve 10 values along the z-axis from the minimum z coordinate to maximum 
    /// z coordinate (i.e. a value at both the minimum z and maximum z coordinates and 8 values between).
    /// </summary>
    [FromQuery(Name = "resolution-z")] public double? resZ { get; set; }
}
