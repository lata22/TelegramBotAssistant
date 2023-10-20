using CognitiveServices.AI.Services.GoogleSearch;
using CognitiveServices.AI.Services.SemanticKernel.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticMemory.AI.Tokenizers.GPT3;
using Newtonsoft.Json;

namespace CognitiveServices.AI.Services.SemanticKernel;

public class ContentSummarizer : IContentSummarizer
{
    private readonly IKernel _semanticKernel;

    public ContentSummarizer(IKernel semanticKernel)
    {
        _semanticKernel = semanticKernel;
    }

    public async Task<List<WebsiteResult>> SummarizeWebsiteResults(List<WebsiteResult> websiteResults, int totalTokenLimit, CancellationToken cancellationToken)
    {
        var json = JsonConvert.SerializeObject(websiteResults);
        if (GPT3Tokenizer.Encode(json).Count <= totalTokenLimit)
        {
            return websiteResults;
        }
        var websiteResultSummarized = new List<WebsiteResult>();
        int tokenLimitPerWebsiteResult = totalTokenLimit / websiteResults.Count;
        foreach (var websiteResult in websiteResults)
        {
            int titleTokenCount = GPT3Tokenizer.Encode(websiteResult.Title).Count;
            int urlTokenCount = GPT3Tokenizer.Encode(websiteResult.URL).Count;
            int contentTokenCount = tokenLimitPerWebsiteResult - titleTokenCount - urlTokenCount;

            if (GPT3Tokenizer.Encode(websiteResult.ToString()).Count > tokenLimitPerWebsiteResult)
            {
                var paragraphList = websiteResult.Content.Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries).ToList();
                int maxTokensPerParagraph = contentTokenCount / paragraphList.Count;
                var chunks = TextChunker.SplitPlainTextParagraphs(paragraphList, maxTokensPerParagraph, 0, null, input => GPT3Tokenizer.Encode(input).Count);
                var tasks = chunks.ToList().Select(chunk => CreateAndExecuteSummarizationFunction(chunk, maxTokensPerParagraph, cancellationToken));
                var summarizedChunks = await Task.WhenAll(tasks);
                websiteResultSummarized.Add(new WebsiteResult(websiteResult.Title, websiteResult.URL, string.Join("\n", summarizedChunks)));
            }
        }
        return websiteResultSummarized;
    }

    private async Task<string> CreateAndExecuteSummarizationFunction(string textToSummarize, int tokenCount, CancellationToken cancellationToken)
    {
        var requestSettings = new OpenAIRequestSettings()
        {
            MaxTokens = tokenCount,
            Temperature = 0.3,
            TopP = 0.4,
            PresencePenalty = 0.5,
            FrequencyPenalty = 0.5,
        };
        var summarizationFunction = _semanticKernel.CreateSemanticFunction(SemanticFunctions.TextChunkSummarizationPrompt(tokenCount),
            nameof(SemanticFunctions.TextChunkSummarizationPrompt),
           "SummarizationSkill",
           string.Empty,
           requestSettings);
        var context = _semanticKernel.CreateNewContext();
        context.Variables.Add("TextToSummarize", textToSummarize);
        var functionResult = await summarizationFunction.InvokeAsync(context, null, cancellationToken);
        var functionValue = functionResult.GetValue<string>();
        if (functionValue != null)
        {
            return functionValue;
        }

        return string.Empty;
    }
}
