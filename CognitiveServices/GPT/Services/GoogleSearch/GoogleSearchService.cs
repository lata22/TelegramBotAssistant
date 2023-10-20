using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using HtmlAgilityPack;
using Infrastructure.Firebase.Config;
using Microsoft.Extensions.Logging;

namespace CognitiveServices.AI.Services.GoogleSearch;

public class GoogleSearchService : IGoogleSearchService
{
    private readonly CustomSearchAPIService _customSearchAPIService;
    private readonly GoogleConfig _googleConfig;
    private readonly ILogger<GoogleSearchService> _logger;
    public GoogleSearchService(
        CustomSearchAPIService customSearchAPIService,
        GoogleConfig googleConfig,
        ILogger<GoogleSearchService> logger)
    {
        _customSearchAPIService = customSearchAPIService;
        _googleConfig = googleConfig;
        _logger = logger;
    }

    public async Task<List<WebsiteResult>> GetWebsiteResults(string query, int limit = 3, int skip = 0, CancellationToken cancellationToken = default)
    {
        var search = await GetGoogleSearch(query, limit, skip, cancellationToken);
        var websiteResults = new List<WebsiteResult>();
        foreach (Result item in search.Items)
        {
            try
            {
                using var httpClient = new HttpClient();
                string htmlContent = await httpClient.GetStringAsync(item.Link, cancellationToken);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                // Extract specific content, like all paragraph texts
                var paragraphNodes = htmlDocument.DocumentNode.SelectNodes("//p");

                if (paragraphNodes != null)
                {
                    string content = string.Join('\n', paragraphNodes.Select(pn => pn.InnerText));
                    websiteResults.Add(new WebsiteResult(item.Title, item.Link, content));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ex.Message);
            }
        }
        return websiteResults;
    }

    private async Task<Search> GetGoogleSearch(string query, int limit, int skip, CancellationToken cancellationToken)
    {
        // Create a CseResource.ListRequest object
        CseResource.ListRequest listRequest = _customSearchAPIService.Cse.List();
        listRequest.Cx = _googleConfig.SearchEngineId;
        listRequest.Q = query;
        listRequest.Num = limit;
        listRequest.Start = skip;

        // Execute the search
        return await listRequest.ExecuteAsync(cancellationToken);
    }
}

