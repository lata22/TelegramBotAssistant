namespace CognitiveServices.AI.Services.GoogleSearch
{
    public interface IGoogleSearchService
    {
        Task<List<WebsiteResult>> GetWebsiteResults(string query, int limit = 3, int skip = 0, CancellationToken cancellationToken = default);
    }
}
