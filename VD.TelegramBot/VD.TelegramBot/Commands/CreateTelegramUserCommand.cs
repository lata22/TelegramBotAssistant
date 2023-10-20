using Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VD.TelegramBot.Db.Entities;
using VD.TelegramBot.Db.Interfaces;

namespace VD.TelegramBot.Commands
{
    public record CreateTelegramUserCommand(long ChatId, string Username) : IRequest<Unit>;

    public class CreateTelegramUserCommandHandler : IRequestHandler<CreateTelegramUserCommand, Unit>
    {
        private readonly IDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CreateTelegramUserCommandHandler> _logger;
        public CreateTelegramUserCommandHandler(
            IDbContext dbContext,
            ILogger<CreateTelegramUserCommandHandler> logger,
            IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(CreateTelegramUserCommand request, CancellationToken cancellationToken)
        {
            var telegramUser = await _dbContext.TelegramUsers
                .FirstOrDefaultAsync(tu => tu.ChatId == request.ChatId, cancellationToken);

            if (telegramUser is null)
            {
                var entity = new TelegramUser()
                {
                    CreatedAt = DateTime.UtcNow,
                    Username = request.Username,
                    ChatId = request.ChatId,
                    Enabled = true
                };
                try
                {
                    await _dbContext.AddAsync(entity, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await _publishEndpoint.Publish(new ChatSessionCreated(request.ChatId), cancellationToken);
                    _logger.LogInformation($"Username {request.Username} created the subcription to notifications");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
            return Unit.Value;
        }
    }
}
