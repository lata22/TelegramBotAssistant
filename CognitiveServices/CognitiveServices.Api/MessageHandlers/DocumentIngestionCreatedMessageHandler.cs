using CognitiveServices.Db;
using CognitiveServices.Db.Entities;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;

namespace CognitiveServices.Api.MessageHandlers
{
    public class DocumentIngestionCreatedMessageHandler : BaseMessageHandler<DocumentIngestionCreated>
    {
        private readonly ApplicationDBContext _dbContext;

        public DocumentIngestionCreatedMessageHandler(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task Consume(ConsumeContext<DocumentIngestionCreated> context)
        {
            await _dbContext.AddAsync(new DocumentIngestion
            {
                CreatedAt = DateTime.UtcNow,
                DocumentId = context.Message.DocumentId,
                FileName = context.Message.FileName,
                TelegramChatid = context.Message.TelegramChatId,
            }, context.CancellationToken);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
