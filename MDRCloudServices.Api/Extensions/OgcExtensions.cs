using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MDRCloudServices.Api.Controllers.OGC;
using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.Helpers.Hyperlinkr;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization;

namespace MDRCloudServices.Api.Extensions;

public static class OgcExtensions
{
    /// <summary>
    /// Cloning of the CRS List is required for YAML to serialise without shortcuts
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<Crs> Clone(this List<Crs> items)
    {
        return items.Select(x => new Crs { Name = x.Name, Wkt = x.Wkt }).ToList();
    }

    public static async Task SetLinksAsync(this OgcCollection collection, IUrlHelper url, IMediator m, string format)
    {
        var crsList = await m.Send(new CrsListQuery());

        collection.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new())).GetLeftPart(UriPartial.Authority)
                + url.Content("~/index.html") + "?urls.primaryName=Cefas%20OGC%20Environmental%20Data%20Retrieval%20API",
            Rel = "service-doc",
            Type = "text/html",
            Title = "Cefas OGC Environmental Data Retrieval API"
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new())).GetLeftPart(UriPartial.Authority)
                + url.Content("~/swagger/ogc/swagger.json"),
            Rel = "service-desc",
            Type = "text/html",
            Title = "Cefas OGC Environmental Data Retrieval API"
        });

        collection.Links.Add(await m.Send(new LicenceLinkQuery(int.Parse(collection.Id ?? "0"))));
        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionsController, IActionResult>(a => a.CollectionItemAsync(collection.Id ?? string.Empty, format)).ToString(),
            Rel = "collection",
            Type = "collection",
            Title = "Collection"
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
            Rel = "data",
            Type = "position",
            Title = "Position"
        });
        collection.DataQueries.Add("position", new()
        {
            Link = new()
            {
                Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
                Rel = "alternative",
                Type = "application/geo+json",
                Title = "Position",
                Variables = new()
                {
                    new()
                    {
                        OutputFormats = new() { "GeoJSON" },
                        Description = "Query to return data for a defined well known text point",
                        QueryType = "Position",
                        Title = "Position Query",
                        CrsDetails = crsList.Clone(),
                    }
                }
            }
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.AreaAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, null, null, CancellationToken.None)).ToString(),
            Rel = "data",
            Type = "area",
            Title = "Area"
        });
        collection.DataQueries.Add("area", new()
        {
            Link = new()
            {
                Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
                Rel = "alternative",
                Type = "application/geo+json",
                Title = "Area",
                Variables = new()
                {
                    new()
                    {
                        OutputFormats = new() { "GeoJSON" },
                        Description = "Query to return data for a defined well known text geometry",
                        QueryType = "Area",
                        Title = "Area Query",
                        WithinUnits = new(OgcQueryBuilder.DistanceUnits),
                        CrsDetails = crsList.Clone(),
                    }
                }
            }
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.RadiusAsync(collection.Id ?? string.Empty, new() { f = format }, string.Empty, 1, "km", CancellationToken.None)).ToString(),
            Rel = "data",
            Type = "radius",
            Title = "Radius"
        });
        collection.DataQueries.Add("radius", new()
        {
            Link = new()
            {
                Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
                Rel = "alternative",
                Type = "application/geo+json",
                Title = "Radius",
                Variables = new()
                {
                    new()
                    {                        
                        OutputFormats = new() { "GeoJSON" },
                        Description = "Query to return data withing a distance from a defined well known text point",
                        QueryType = "Radius",
                        Title = "Radius Query",
                        WithinUnits = new(OgcQueryBuilder.DistanceUnits),
                        CrsDetails = crsList.Clone(),
                    }
                }
            }
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.CubeAsync(collection.Id ?? string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
            Rel = "data",
            Type = "cube",
            Title = "Cube"
        });
        collection.DataQueries.Add("cube", new()
        {
            Link = new()
            {
                Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
                Rel = "alternative",
                Type = "application/geo+json",
                Title = "Cube",
                Variables = new()
                {
                    new()
                    {
                        OutputFormats = new() { "GeoJSON" },
                        Description = "Query to return data for a 3D bounding box",
                        QueryType = "Cube",
                        Title = "Cube Query",
                        CrsDetails = crsList.Clone(),
                    }
                }
            }
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.TrajectoryAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
            Rel = "data",
            Type = "trajectory",
            Title = "Trajectory"
        });
        collection.DataQueries.Add("trajectory", new()
        {
            Link = new()
            {
                Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
                Rel = "alternative",
                Type = "application/geo+json",
                Title = "trajectory",
                Variables = new()
                {
                    new()
                    {
                        OutputFormats = new() { "GeoJSON" },
                        Description = "Query to return data for a defined well known text line",
                        QueryType = "Trajectory",
                        Title = "Trajectory Query",
                        CrsDetails = crsList.Clone(),
                    }
                }
            }
        });

        collection.Links.Add(new()
        {
            Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.CorridorAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, new(), CancellationToken.None)).ToString(),
            Rel = "data",
            Type = "corridor",
            Title = "Corridor"
        });
        collection.DataQueries.Add("corridor", new()
        {
            Link = new()
            {
                Href = url.GetLink<CollectionDataQueryController, IActionResult>(a => a.PositionAsync(collection.Id ?? string.Empty, string.Empty, new() { f = format }, CancellationToken.None)).ToString(),
                Rel = "alternative",
                Type = "application/geo+json",
                Title = "Corridor",
                Variables = new()
                {
                    new()
                    {
                        WidthUnits = new(OgcQueryBuilder.DistanceUnits),
                        OutputFormats = new() { "GeoJSON" },
                        Description = "Query to return data for a defined well known text polygon",
                        QueryType = "Corridor",
                        Title = "Corridor Query",
                        HeightUnits = new(OgcQueryBuilder.DistanceUnits),
                        CrsDetails = crsList.Clone(),
                    }
                }
            }
        });
    }

    public static void SetLinks(this OgcCapabilities capabilities, IUrlHelper url, FormatQuery query)
    {
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(query)).ToString(),
            Rel = "self",
            Type = (query.f ?? "json").ToLowerInvariant() switch
            {
                "json" => "application/json",
                "yaml" => "application/yaml",
                "xml" => "application/xml",
                "html" => "text/html",
                _ => throw new ValidationException(new ErrorResponse { { HttpStatusCode.BadRequest, "Bad format", "", "" } })
            }
        });

        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new())).GetLeftPart(UriPartial.Authority)
                + url.Content("~/index.html") + "?urls.primaryName=Cefas%20OGC%20Environmental%20Data%20Retrieval%20API",
            Rel = "service-doc",
            Type = "text/html",
            Title = "Cefas OGC Environmental Data Retrieval API"
        });

        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new())).GetLeftPart(UriPartial.Authority)
                + url.Content("~/swagger/ogc/swagger.json"),
            Rel = "service-desc",
            Type = "text/html",
            Title = "Cefas OGC Environmental Data Retrieval API"
        });

        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new() { f = "json" })).ToString(),
            Hreflang = "en",
            Rel = "alternative",
            Type = "application/json"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new() { f = "yaml" })).ToString(),
            Hreflang = "en",
            Rel = "alternative",
            Type = "application/yaml"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new() { f = "xml" })).ToString(),
            Hreflang = "en",
            Rel = "alternative",
            Type = "application/xml"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.IndexAsync(new() { f = "html" })).ToString(),
            Hreflang = "en",
            Rel = "alternative",
            Type = "text/html"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.Conformance("json")).ToString(),
            Rel = "conformance",
            Type = "application/json",
            Title = "OGC conformance classes implemented by this API"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.Conformance("xml")).ToString(),
            Rel = "conformance",
            Type = "application/xml",
            Title = "OGC conformance classes implemented by this API"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.Conformance("yaml")).ToString(),
            Rel = "conformance",
            Type = "application/yaml",
            Title = "OGC conformance classes implemented by this API"
        });
        capabilities.Links.Add(new()
        {
            Href = url.GetLink<CapabilitiesController, IActionResult>(a => a.Conformance("html")).ToString(),
            Rel = "conformance",
            Type = "application/html",
            Title = "OGC conformance classes implemented by this API"
        });
    }

    public static string SerialiseObjectAsXml<T>(this T toSerialise)
    {
        if (toSerialise == null) throw new ArgumentNullException(nameof(toSerialise));
        var xmlSerializer = new XmlSerializer(toSerialise.GetType());
        using StringWriter textWriter = new();
        xmlSerializer.Serialize(textWriter, toSerialise);
        return textWriter.ToString();
    }


    public static string SerialiseObjectAsYaml<T>(this T toSerialise) => SerialiseObjectAsYaml(toSerialise, new LowerCaseNamingConvention());

    public static string SerialiseObjectAsYaml<T>(this T toSerialise, INamingConvention convention)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(convention)
            .Build();
        return serializer.Serialize(toSerialise);
    }    

    public static IActionResult SerialiseFormat<T>(this T toSerialise, Controller controller, string format)
    {
        if (toSerialise == null) return new NotFoundResult();
        return format.ToLowerInvariant() switch
        {
            "json" => controller.Json(toSerialise),
            "xml" => controller.Content(toSerialise.SerialiseObjectAsXml(), "application/xml"),
            "html" => controller.View("model", toSerialise),
            "yaml" => controller.Content(toSerialise.SerialiseObjectAsYaml(), "application/yaml"),
            _ => controller.BadRequest("Unknown format")
        };
    }
}
