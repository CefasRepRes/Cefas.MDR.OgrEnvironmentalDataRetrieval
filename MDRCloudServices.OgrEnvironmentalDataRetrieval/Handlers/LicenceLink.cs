using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get licence link for a recordset</summary>
public record LicenceLinkQuery(int recordsetId) : IRequest<OgcLink>;

/// <summary>Handler for get licence link for a recordset query</summary>
public class LicenceLinkHandler : IRequestHandler<LicenceLinkQuery, OgcLink>
{
    private readonly IDatabase _db;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    public LicenceLinkHandler(IDatabase db)
    {
        _db = db;
    }

    /// <summary>Handle get licence link for a recordset query</summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<OgcLink> Handle(LicenceLinkQuery request, CancellationToken cancellationToken)
    {
        var query = new Query()
            .GenerateSelect(typeof(Licence))
            .Join("Recordsets.Recordsets as b", "a.Id", "b.Licence")
            .Where("b.Id", request.recordsetId);

        var licence = (await _db.FetchAsync<Licence>(query)).FirstOrDefault();

        if (licence == null)
        {
            return new OgcLink()
            {
                Href = "http://www.nationalarchives.gov.uk/doc/open-government-licence/",
                Hreflang = "en",
                Rel = "licence",
                Title = "",
                Type = "text/html"
            };
        }

        return new OgcLink()
        {
            Href = licence.Href,
            Hreflang = "en",
            Rel = "licence",
            Title = licence.Name,
            Type = "text/html"
        };
    }
}
