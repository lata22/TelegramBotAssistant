using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Extensions;
using VD.TelegramBot.Db.Interfaces;

namespace VD.TelegramBot.Commands
{
    public record TelegramBotVideoNotificationCommand(byte[] Video, long ChatId) : IRequest<Unit>;
    public class TelegramBotVideoNotificationCommandHandler : IRequestHandler<TelegramBotVideoNotificationCommand, Unit>
    {
        private readonly IDbContext _dbContext;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramBotVideoNotificationCommandHandler> _logger;

        public TelegramBotVideoNotificationCommandHandler(
            IDbContext dbContext,
            ITelegramBotClient telegramBotClient,
            ILogger<TelegramBotVideoNotificationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(TelegramBotVideoNotificationCommand request, CancellationToken cancellationToken)
        {
            var exist = await _dbContext.TelegramUsers
                 .AnyAsync(tu => tu.ChatId == request.ChatId, cancellationToken);
            if (exist)
            {
                await _telegramBotClient.SendVideoAsync(new ChatId(request.ChatId), request.Video, cancellationToken);
            }
            else
            {
                _logger.LogWarning($"There are no recipients to send the video");
            }
            return Unit.Value;
        }
    }
}
