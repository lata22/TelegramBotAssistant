using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBot.Extensions;
using VD.TelegramBot.Db.Interfaces;

namespace VD.TelegramBot.Commands
{
    public record TelegramBotMessageNotificationCommand(string Message, long ChatId) : IRequest<Unit>;
    public class TelegramBotMessageNotificationCommandHandler : IRequestHandler<TelegramBotMessageNotificationCommand, Unit>
    {
        private readonly IDbContext _dbContext;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramBotMessageNotificationCommandHandler> _logger;
        public TelegramBotMessageNotificationCommandHandler(
            IDbContext dbContext,
            ITelegramBotClient telegramBotClient,
            ILogger<TelegramBotMessageNotificationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(TelegramBotMessageNotificationCommand request, CancellationToken cancellationToken)
        {
            var exist = await _dbContext.TelegramUsers
                      .AnyAsync(tu => tu.ChatId == request.ChatId, cancellationToken);
            if (exist)
            {
                await _telegramBotClient.SendTextAsync(request.ChatId, request.Message, cancellationToken);
            }
            else
            {
                _logger.LogWarning($"There are no recipients to send the telegram notification: {request.Message}");
            }

            return Unit.Value;
        }
    }

}
