namespace TelegramBot.Interfaces
{
    public interface ITelegramBotQueryDispatcher : ITelegramBotSingleton
    {
        public Task<string> DispatchQuery(string telegramCommand, CancellationToken cancellation);
    }
}
