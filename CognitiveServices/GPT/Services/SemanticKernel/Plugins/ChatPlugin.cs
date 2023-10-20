using CognitiveServices.AI.Services.GPT;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins
{
    public class ChatPlugin : BasePlugin
    {
        private readonly IGPTCompletionService _GPTCompletionService;
        private readonly ILogger<ChatPlugin> _logger;
        public ChatPlugin(
            ILogger<ChatPlugin> logger, IGPTCompletionService GPTCompletionService)
        {
            _logger = logger;
            _GPTCompletionService = GPTCompletionService;
        }

        [SKFunction, Description("Send a prompt to the LLM. Do not use it if the user does not ask for it explicitly")]
        public async Task<string> SendPromptAsync(
            SKContext context, 
            [Description("The prompt to be sent to the LLM")] string prompt, 
            CancellationToken cancellationToken)
        {
            return await ExecuteWithAbortCheckAsync(context, async () =>
            {
                _logger.LogInformation($"Calling PromptAsync function with prompt: {prompt}");
                string reply;
                try
                {
                    long chatId = GetChatIdFromContext(context);
                    reply = await _GPTCompletionService.GetBeutifullResponse(
                        chatId,
                        "You are a friendly, intelligent, and curious assistant who is good at conversation.\n If you don't have sufficient information, reply with 'INFO NOT FOUND'",
                        prompt,
                        string.Empty,
                        cancellationToken,
                        true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    reply = $"OpenAI returned an error ({ex.Message}). Please try again.";
                }
                return reply;
            });
        }
    }
}
