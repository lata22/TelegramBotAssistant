using MediatR;
using Telegram.Bot.Types;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotMessageHandler : ITelegramBotSingleton
    {
        Task<Unit> HandleMessage(Message message, CancellationToken cancellationToken);
    }
}
