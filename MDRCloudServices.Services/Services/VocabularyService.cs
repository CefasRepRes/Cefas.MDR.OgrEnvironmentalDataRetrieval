using MDRCloudServices.Services.Interfaces;
using MDRDB.MDR;
using NPoco;

namespace MDRCloudServices.Services.Services;

/// <summary>Vocabulary Service</summary>
public class VocabularyService : IVocabularyService
{
    private readonly IDatabase _db;

    /// <summary>Default constructor</summary>
    /// <param name="db"></param>
    /// <param name="vocabularyValidationService"></param>
    public VocabularyService(IDatabase db)
    {
        _db = db;        
    }

    public async Task<List<Keyword>> GetKeywordsForVocabularyAsync(string vocab)
    {
        return await _db.FetchAsync<Keyword>("WHERE vocabulary = @0 AND EndDate IS NULL ORDER BY [Order]", vocab);
    }
}

