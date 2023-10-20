using MediatR;
using Telegram.Bot.Types;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotAudioMessageHandler : ITelegramBotSingleton
    {
        Task<Unit> HandleAudioMessage(Message message, CancellationToken cancellationToken);
    }
}
