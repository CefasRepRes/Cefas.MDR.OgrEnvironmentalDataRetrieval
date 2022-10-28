using MDRDB.MDR;

namespace MDRCloudServices.Services.Interfaces;

/// <summary>Vocabulary Service</summary>
public interface IVocabularyService
{
    Task<List<Keyword>> GetKeywordsForVocabularyAsync(string vocab);
}
