using MediatR;
using Telegram.Bot.Types;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotAccountHandler : ITelegramBotSingleton
    {
        public Task<Unit> Handle(Message message, CancellationToken cancellationToken);
    }
}
