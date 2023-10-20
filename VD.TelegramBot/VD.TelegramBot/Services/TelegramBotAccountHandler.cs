using MediatR;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.Services
{
    public class TelegramBotAccountHandler : ITelegramBotAccountHandler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TelegramBotAccountHandler> _logger;
        public TelegramBotAccountHandler(IMediator mediator, ILogger<TelegramBotAccountHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new CreateTelegramUserCommand(message.Chat.Id, message.Chat.Username!), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return Unit.Value;
        }
    }
}
