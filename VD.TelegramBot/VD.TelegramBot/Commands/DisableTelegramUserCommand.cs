using MediatR;
using Microsoft.EntityFrameworkCore;
using VD.TelegramBot.Db.Interfaces;

namespace VD.TelegramBot.Commands
{
    public record DisableTelegramUserCommand(long ChatId) : IRequest<Unit>;

    public class DisableTelegramUserCommandHandler : IRequestHandler<DisableTelegramUserCommand, Unit>
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<DisableTelegramUserCommandHandler> _logger;
        public DisableTelegramUserCommandHandler(
            IDbContext dbContext,
            ILogger<DisableTelegramUserCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(DisableTelegramUserCommand request, CancellationToken cancellationToken)
        {
            var telegramUser = await _dbContext.TelegramUsers
                .FirstOrDefaultAsync(tu => tu.ChatId == request.ChatId, cancellationToken);

            if (telegramUser != null)
            {
                telegramUser.Enabled = false;
                var userName = telegramUser.Username;
                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"Username {userName} is now disabled");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }
            return Unit.Value;
        }
    }
}
