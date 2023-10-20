using CognitiveServices.Db;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CognitiveServices.Api.MessageHandlers
{
    public class ChatSessionCreatedMessageHandler : BaseMessageHandler<ChatSessionCreated>
    {
        private readonly ApplicationDBContext _dbContext;

        public ChatSessionCreatedMessageHandler(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async override Task Consume(ConsumeContext<ChatSessionCreated> context)
        {
            if (!await _dbContext.ChatSessions.AnyAsync(cs => cs.Id == context.Message.TelegramChatId, context.CancellationToken))
            {
                await _dbContext.AddAsync(new Db.Entities.ChatSession()
                {
                    Id = context.Message.TelegramChatId,
                    CreatedAt = DateTime.UtcNow,
                }, context.CancellationToken);
                await _dbContext.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
