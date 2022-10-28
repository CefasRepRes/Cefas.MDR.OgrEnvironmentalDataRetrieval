using MDRCloudServices.DataLayer.SqlKata;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Handlers;

/// <summary>Get keywords from holding for recordset</summary>
/// <param name="RecordsetId"></param>
public record KeywordsForCollectionQuery(int RecordsetId) : IRequest<List<string>>;

/// <summary>Handler for Getting keywords from holding for recordset</summary>
public class KeywordsForCollectionHandler : IRequestHandler<KeywordsForCollectionQuery, List<string>>
{
    private readonly IDatabase _db;

    /// <summary>Default Constructor</summary>
    /// <param name="db"></param>
    public KeywordsForCollectionHandler(IDatabase db)
    {
        _db = db;
    }

    /// <summary>Get keywords from holding for recordset</summary>
    public async Task<List<string>> Handle(KeywordsForCollectionQuery request, CancellationToken cancellationToken)
    {
        var query = new Query("Recordsets.Recordsets AS R")
            .Distinct()
            .Select(new string[] { "K.DisplayName" })
            .Join("MDR.HoldingProperties AS P", j => j.On("R.HoldingId", "P.HoldingId").Where("P.Vocabulary", "INSPIRE"))
            .Join("MDR.Keywords AS K", j => j.On("P.Value", "K.Name").On("P.Vocabulary", "K.Vocabulary"))
            .Where("R.Id", request.RecordsetId)
            .WhereNull("P.EndDate");

        return await _db.FetchAsync<string>(query);
    }
}
