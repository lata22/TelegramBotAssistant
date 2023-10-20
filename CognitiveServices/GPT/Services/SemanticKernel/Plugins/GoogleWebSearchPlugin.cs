using CognitiveServices.AI.Services.GoogleSearch;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticMemory.AI.Tokenizers.GPT3;
using Newtonsoft.Json;
using System.ComponentModel;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;

public sealed class GoogleWebSearchPlugin : BasePlugin
{
    private readonly IGoogleSearchService _customSearchAPIService;
    private readonly IKernel _kernel;
    private readonly IContentSummarizer _contentSummarizer;
    public GoogleWebSearchPlugin(
        IGoogleSearchService customSearchAPIService,
        IKernel kernel,
        IContentSummarizer contentSummarizer)
    {
        _customSearchAPIService = customSearchAPIService;
        _kernel = kernel;
        _contentSummarizer = contentSummarizer;
    }

    [SKFunction, Description("Performs a Google web search based on the user's query and returns a consolidated summary.")]
    public async Task<string> SearchInGoogleAndSummarizeAsync(
        SKContext context,
        CancellationToken cancellationToken,
        [Description("The search query provided by the user for which a summary will be generated.")] string query,
        [Description("Optional number of results to limit")] int limit = 3,
        [Description("Optional number of results to skip")] int skip = 0)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            string goal = GetGoalFromContext(context);
            if (limit < 3)
            {
                limit = 3;
            }
            string summary = await GetWebsitesContent(context, query, goal, limit, skip, cancellationToken);
            return summary ?? string.Empty;
        });
    }

    [SKFunction, Description("Performs a Google web search to specifically extract a direct answer to the user's query.")]
    public async Task<string> SearchInGoogleAsync(
    SKContext context,
    CancellationToken cancellationToken,
    [Description("The search query provided by the user for which a specific answer will be extracted.")] string query,
    [Description("Optional number of results to limit")] int limit = 3,
    [Description("Optional number of results to skip")] int skip = 0)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            string goal = GetGoalFromContext(context);
            if (limit < 3)
            {
                limit = 3;
            }
            string summary = await GetAnswerFromWebsites(context, query, goal, limit, skip, cancellationToken);
            return summary ?? string.Empty;
        });
    }

    private async Task<string> GetAnswerFromWebsites(SKContext context, string query, string goal, int limit, int skip, CancellationToken cancellationToken)
    {
        var websiteResults = await _customSearchAPIService.GetWebsiteResults(query, limit, skip, cancellationToken);
        if (websiteResults.Count > 0)
        {
            int tokenLimit = 16385 - GPT3Tokenizer.Encode(SemanticFunctions.ExtractAnswerFromJsonDataToAGivenUserPrompt(goal)).Count;
            var textWebsiteResult = string.Join(' ', websiteResults.Select(wr => wr.ToString()));
            textWebsiteResult = new string(textWebsiteResult.Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ').ToArray());
            if (GPT3Tokenizer.Encode(textWebsiteResult).Count > tokenLimit)
            {
                websiteResults = await _contentSummarizer.SummarizeWebsiteResults(websiteResults, tokenLimit, cancellationToken);
            }
            var json = JsonConvert.SerializeObject(websiteResults, Formatting.Indented);
            var summary = await ExecuteSemanticFunction(
                context,
                SemanticFunctions.ExtractAnswerFromJsonDataToAGivenUserPrompt(goal),
                nameof(SemanticFunctions.ExtractAnswerFromJsonDataToAGivenUserPrompt),
                json,
                cancellationToken);
            return summary;
        }
        return string.Empty;
    }

    private async Task<string> GetWebsitesContent(SKContext context, string query, string goal, int limit, int skip, CancellationToken cancellationToken)
    {
        var websiteResults = await _customSearchAPIService.GetWebsiteResults(query, limit, skip, cancellationToken);
        // Handle search results
        if (websiteResults.Count > 0)
        {
            int tokenLimit = 16385 - GPT3Tokenizer.Encode(SemanticFunctions.ExtractAnswerFromJsonDataToAGivenUserPrompt(goal)).Count;
            var textWebsiteResult = string.Join(' ', websiteResults.Select(wr => wr.ToString()));
            if (GPT3Tokenizer.Encode(textWebsiteResult).Count > tokenLimit)
            {
                websiteResults = await _contentSummarizer.SummarizeWebsiteResults(websiteResults, tokenLimit, cancellationToken);
            }
            var json = JsonConvert.SerializeObject(textWebsiteResult, Formatting.Indented);
            int summaryTokenCount = 2000;
            var summary = await ExecuteSemanticFunction(
                context,
                SemanticFunctions.ConsolidatedSummaryFromWebsiteResults(goal, summaryTokenCount),
                nameof(SemanticFunctions.ConsolidatedSummaryFromWebsiteResults),
                json,
                cancellationToken);
            return summary;
        }
        return string.Empty;
    }

    private async Task<string> ExecuteSemanticFunction(
        SKContext context,
        string semanticFunction,
        string name,
        string input,
        CancellationToken cancellationToken = default)
    {
        var niceSummarizationMessage = _kernel.CreateSemanticFunction(
                semanticFunction,
                name,
                nameof(GoogleWebSearchPlugin),
                string.Empty);
        context.Variables.Add("Input", input);
        var summary = await niceSummarizationMessage.InvokeAsync(context, null, cancellationToken);
        string? summaryValue = summary.GetValue<string>();
        if (summaryValue != null)
        {
            return summaryValue;
        }
        SetAbortContextVariable(context);
        return string.Empty;
    }
}
