using CognitiveServices.AI.Services.GPT;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;
public class TranslatePlugin : BasePlugin
{
    private readonly IGPTCompletionService _GPTCompletionService;

    public TranslatePlugin(
        IGPTCompletionService gPTCompletionService)
    {
        _GPTCompletionService = gPTCompletionService;
    }

    [SKFunction, Description("Translates the given input into the same user prompt language. Do not use it if the user does not ask for it explicitly")]
    public async Task<string> Translate(
        SKContext context,
        [Description("Input to be translated")] string input,
        [Description("Language to be translated")] string language,
        CancellationToken cancellationToken)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            var (goal, chatId) = GetGoalAndChatIdFromContext(context);
            return await _GPTCompletionService.GetBeutifullResponse(
                chatId,
                "You are a professional translator",
                goal,
                input,
                cancellationToken);
        });
    }
}
