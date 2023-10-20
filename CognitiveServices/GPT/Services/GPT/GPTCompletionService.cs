using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json;

namespace CognitiveServices.AI.Services.GPT
{
    public class GPTCompletionService : IGPTCompletionService
    {
        private readonly IChatCompletion _chatCompletion;
        private readonly ISemanticTextMemory _memoryClient;
        public GPTCompletionService(IKernel kernel, ISemanticTextMemory memoryClient)
        {
            _chatCompletion = kernel.GetService<IChatCompletion>();
            _memoryClient = memoryClient;
        }

        public async Task<string> GetBeutifullResponse(long chatId, string systemPrompt, string originalPrompt, string promptResult, CancellationToken cancellationToken, bool persistChat = false)
        {
            var chatHistory = new ChatHistory();
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                chatHistory.AddSystemMessage(systemPrompt);
            }

            string userMessage = string.IsNullOrWhiteSpace(promptResult) ?
                    originalPrompt :
                    $"Prompt:{originalPrompt}\nResult: {promptResult}";
            chatHistory.AddUserMessage(userMessage);

            var result = await _chatCompletion.GenerateMessageAsync(
                chatHistory,
                null,
                cancellationToken);
            chatHistory.AddAssistantMessage(result);

            if (persistChat)
            {
                var recentMessages = chatHistory.Where(ch => ch.Role == AuthorRole.User || ch.Role == AuthorRole.Assistant);
                string contetInJsonFormat = JsonConvert.SerializeObject(recentMessages, Formatting.Indented);
                await _memoryClient.SaveInformationAsync(
                    $"ChatHistory_{chatId}",
                    contetInJsonFormat,
                    Guid.NewGuid().ToString(),
                    "ChatHistory",
                    DateTime.UtcNow.ToString(),
                    cancellationToken);
            }
            return result;
        }
    }
}
