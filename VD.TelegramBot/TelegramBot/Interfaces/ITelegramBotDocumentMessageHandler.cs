using MediatR;
using Telegram.Bot.Types;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotDocumentMessageHandler: ITelegramBotSingleton
    {
        Task<Unit> HandleDocumentMessage(Message message, CancellationToken cancellationToken);
    }
}
