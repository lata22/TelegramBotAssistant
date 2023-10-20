namespace TelegramBot.Interfaces
{
    public interface ITelegramMessageTextHandler : ITelegramBotSingleton
    {
        Task<bool> CanHandle(string message);
        Task Handle(string message, long chatId, CancellationToken cancellationToken);
    }
}
