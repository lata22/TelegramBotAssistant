using Telegram.Bot;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotErrorHandler : ITelegramBotSingleton
    {
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
    }
}
