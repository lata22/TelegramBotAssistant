using Telegram.Bot;
using Telegram.Bot.Args;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotOnApiResponseHandler : ITelegramBotSingleton
    {
        ValueTask HandleApiResponse(ITelegramBotClient botClient, ApiResponseEventArgs args, CancellationToken cancellationToken = default);
    }
}
