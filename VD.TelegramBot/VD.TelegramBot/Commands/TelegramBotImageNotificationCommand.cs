using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBot.Extensions;
using VD.TelegramBot.Db.Interfaces;

namespace VD.TelegramBot.Commands
{
    public record TelegramBotImageNotificationCommand(byte[] Image, long ChatId) : IRequest<Unit>;

    public class TelegramBotImageNotificationCommandHandler : IRequestHandler<TelegramBotImageNotificationCommand, Unit>
    {
        private readonly IDbContext _dbContext;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramBotImageNotificationCommandHandler> _logger;
        public TelegramBotImageNotificationCommandHandler(
            IDbContext dbContext,
            ITelegramBotClient telegramBotClient,
            ILogger<TelegramBotImageNotificationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(TelegramBotImageNotificationCommand request, CancellationToken cancellationToken)
        {
            var exist = await _dbContext.TelegramUsers
                .AnyAsync(tu => tu.ChatId == request.ChatId, cancellationToken);
            if (exist)
            {
                await _telegramBotClient.SendPhotoAsync(request.ChatId, request.Image, cancellationToken);
            }
            else
            {
                _logger.LogWarning($"There are no recipients to send the image");
            }
            return Unit.Value;
        }
    }
}
