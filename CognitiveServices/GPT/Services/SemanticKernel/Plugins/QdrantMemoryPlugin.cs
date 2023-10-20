using CognitiveServices.AI.Services.SemanticMemory;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticMemory;
using Newtonsoft.Json;
using System.ComponentModel;
namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;

public class QdrantMemoryPlugin : BasePlugin
{
    private readonly ISemanticMemoryClient _memoryClient;
    private readonly ILogger<QdrantMemoryPlugin> _logger;
    private readonly ISemanticTextMemory _semanticTextMemory;

    public QdrantMemoryPlugin(
        ISemanticMemoryClient memoryClient,
        ILogger<QdrantMemoryPlugin> logger,
        ISemanticTextMemory semanticTextMemory)
    {
        _memoryClient = memoryClient;
        _logger = logger;
        _semanticTextMemory = semanticTextMemory;
    }

    [SKFunction, Description("Searches in Qdrant knowledge base for the answer to the given question. Do not use it if the user does not ask for it explicitly")]
    public async Task<string> FindInformationInQdrant(
        SKContext context,
        [Description("Descriptive information to be searched in Qdrant")] string ask,
        CancellationToken cancellationToken)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            long index = GetChatIdFromContext(context);
            var search = await _memoryClient.SearchAsync(ask, index.ToString(), null,null, -1, cancellationToken);
            var answer = await _memoryClient.AskAsync(ask, index.ToString(), null, null, cancellationToken);
            if (answer.Result == "INFO NOT FOUND")
            {
                SetAbortContextVariable(context);
                answer.Result = "Lo siento, no encontré la respuesta, intenta describir mas lo que necesitas";
            }
            return answer.Result;
        });
    }


    [SKFunction, Description("Semantic search and return up to N memories related to the input text with a relevance of 0.5")]
    public async Task<string> RecallAsync(
        SKContext context,
        [Description("The input text to find related memories for")] string input,
        [Description("The maximum number of relevant memories to recall")] int? limit,
        CancellationToken cancellationToken)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            long index = GetChatIdFromContext(context);
            string collection = $"{SemanticTextMemoryPrefix.ChatHistory}_{index}";

            _logger.LogDebug($"Searching memories in collection '{collection}'");

            // Search memory
            List<MemoryQueryResult> memories = await _semanticTextMemory
                .SearchAsync(collection, input, limit ?? 1, 0.5, true, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            if (memories.Count == 0)
            {
                _logger.LogWarning($"Memories not found in collection: {collection}");
                return string.Empty;
            }

            _logger.LogTrace($"Done looking for memories in collection '{collection}')");
            return limit == 1 ? memories[0].Metadata.Text : JsonConvert.SerializeObject(memories.Select(x => x.Metadata.Text));
        });
    }

    [SKFunction, Description("Save information to semantic memory")]
    public async Task<string> SaveAsync(
        SKContext context,
        [Description("The information to save")] string input,
        CancellationToken cancellationToken)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            long index = GetChatIdFromContext(context);
            string collection = $"{SemanticTextMemoryPrefix.ChatHistory}_{index}";
            _logger.LogDebug("Saving memory to collection '{0}'", collection);

            await _semanticTextMemory.SaveInformationAsync(
                collection: collection,
                text: input,
                id: Guid.NewGuid().ToString(),
                cancellationToken: cancellationToken);
            return string.Empty;
        });
    }
}
