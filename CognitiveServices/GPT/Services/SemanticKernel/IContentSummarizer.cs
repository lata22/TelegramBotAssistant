using CognitiveServices.AI.Services.GoogleSearch;

namespace CognitiveServices.AI.Services.SemanticKernel;

public interface IContentSummarizer
{
    Task<List<WebsiteResult>> SummarizeWebsiteResults(List<WebsiteResult> websiteResults, int totalTokenLimit, CancellationToken cancellationToken);
}
