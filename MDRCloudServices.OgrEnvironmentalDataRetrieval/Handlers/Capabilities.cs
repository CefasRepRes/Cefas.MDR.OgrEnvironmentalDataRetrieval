using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MDRCloudServices.Services.Interfaces;
using MediatR;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Capabilities query</summary>
public record CapabilitiesQuery() : IRequest<OgcCapabilities>;

/// <summary>Capabilities handler</summary>
public class CapabilitiesHandler : IRequestHandler<CapabilitiesQuery, OgcCapabilities>
{
    private readonly IVocabularyService _vocabService;

    /// <summary>Default constructor</summary>
    /// <param name="vocabularyService"></param>
    public CapabilitiesHandler(IVocabularyService vocabularyService)
    {
        _vocabService = vocabularyService;
    }

    /// <summary>Handle capabilities query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OgcCapabilities> Handle(CapabilitiesQuery request, CancellationToken cancellationToken)
    {
        var capabilities = new OgcCapabilities
        {
            Title = "Cefas OGC Environmental Data Retrieval API",
            Description = "Access to selected Cefas data via an API that conforms to the OGC Environmental Data Retrieval API specification",
            Provider = new()
            {
                Name = "Cefas",
                Url = "https://www.cefas.co.uk"
            },
            Contact = new()
            {
                Address = "Cefas Lowestoft Laboratory, Pakefield Road",
                City = "Lowestoft",
                Stateorprovince = "Suffolk",
                PostalCode = "NR33 0HT",
                Country = "UK",
                Email = "data.manager@cefas.co.uk",
                Phone = "+44 (0) 1502 562244",
            }
        };

        // Use inspire keywords we have defined.
        var keywords = await _vocabService.GetKeywordsForVocabularyAsync("INSPIRE");
        capabilities.Keywords.AddRange(keywords.Where(x => !string.IsNullOrEmpty(x.ExportName)).Select(x => x.ExportName!));

        return capabilities;
    }
}
