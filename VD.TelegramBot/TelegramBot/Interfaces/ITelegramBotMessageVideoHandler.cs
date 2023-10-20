using MediatR;
using Telegram.Bot.Types;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotVideoMessageHandler : ITelegramBotSingleton
    {
        Task<Unit> HandleVideoMessage(Message message, CancellationToken cancellationToken);
    }

}
