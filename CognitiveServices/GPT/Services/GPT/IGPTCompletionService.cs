namespace CognitiveServices.AI.Services.GPT
{
    public interface IGPTCompletionService
    {
        Task<string> GetBeutifullResponse(long chatId, string systemPrompt, string originalPrompt, string promptResult, CancellationToken cancellationToken, bool persistChat = true);
    }
}
