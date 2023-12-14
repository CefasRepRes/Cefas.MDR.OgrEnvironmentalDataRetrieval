using System.Text.Json;
using MediatR;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Format as GeoJson Query</summary>
/// <param name="id"></param>
/// <param name="items"></param>
public record FormatAsGeoJsonQuery(int id, IAsyncEnumerable<Dictionary<string, object>> items) : IRequest<Stream>;

/// <summary>Format as GeoJson Handler</summary>
public class FormatAsGeoJsonHandler : IRequestHandler<FormatAsGeoJsonQuery, Stream>
{
    private readonly string[] ExcludeFields = new string[] { "__Id", "MDR_Geometry", "CreatedVersion", "DeletedVersion" };

    /// <summary>Handle Formnat as GeoJson query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> Handle(FormatAsGeoJsonQuery request, CancellationToken cancellationToken)
    {
        var wkbReader = new WKBReader();
        var stream = new MemoryStream();
        var jsonWriter = new Utf8JsonWriter(stream);

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("type");
        jsonWriter.WriteStringValue("FeatureCollection");
        jsonWriter.WritePropertyName("features");
        jsonWriter.WriteStartArray();
        await jsonWriter.FlushAsync(cancellationToken);

        int count = 0;
        await foreach (var item in request.items.WithCancellation(cancellationToken))
        {
            var geom = wkbReader.Read(item["MDR_Geometry"] as byte[]);
            var feature = new Feature()
            {
                Geometry = geom,
                Attributes = new AttributesTable(),
                BoundingBox = geom.EnvelopeInternal
            };
            foreach (var key in item.Keys.Where(x => !ExcludeFields.Contains(x)))
            {
                if (item.TryGetValue(key, out var value) && value != null)
                    feature.Attributes.Add(key, value);
            }

            foreach (var attr in feature.Attributes.GetNames())
            {
                if (feature.Attributes[attr] is double d && double.IsInfinity(d))
                    feature.Attributes.DeleteAttribute(attr);
            }

            var serialiser = GeoJsonSerializer.Create();
            var textWriter = new StringWriter();
            serialiser.Serialize(textWriter, feature);
            jsonWriter.WriteRawValue(textWriter.ToString());
            await textWriter.DisposeAsync();
            await jsonWriter.FlushAsync(cancellationToken);
            count++;
        }

        jsonWriter.WriteEndArray();
        jsonWriter.WritePropertyName("numberOfReturned");
        jsonWriter.WriteNumberValue(count);
        jsonWriter.WritePropertyName("numberMatched");
        jsonWriter.WriteNumberValue(count);
        jsonWriter.WriteEndObject();
        await jsonWriter.FlushAsync(cancellationToken);
        await jsonWriter.DisposeAsync();

        stream.Position = 0;

        return stream;
    }
}
