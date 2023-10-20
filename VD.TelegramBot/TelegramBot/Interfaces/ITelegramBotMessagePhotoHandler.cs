using MediatR;
using Telegram.Bot.Types;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotPhotoMessageHandler : ITelegramBotSingleton
    {
        Task<Unit> HandlePhotoMessage(Message message, CancellationToken cancellationToken);
    }
}
