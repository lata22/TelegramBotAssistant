using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.Api.MessageHandlers
{
    public class SDImageCreationProcessedMessageHandler : BaseMessageHandler<SDImageCreationProcessed>
    {
        private readonly IMediator _mediator;
        private readonly ICacheReaderService<byte[]> _cacheReaderService;
        public SDImageCreationProcessedMessageHandler(
            IMediator mediator,
            ICacheReaderService<byte[]> cacheReaderService)
        {
            _mediator = mediator;
            _cacheReaderService = cacheReaderService;
        }

        public override async Task Consume(ConsumeContext<SDImageCreationProcessed> context)
        {
            if (!string.IsNullOrWhiteSpace(context.Message.CacheKey))
            {
                var imageBytes = await _cacheReaderService.GetCachedValueAsync(context.Message.CacheKey);
                if (imageBytes != null)
                {
                    await _mediator.Send(new TelegramBotImageNotificationCommand(imageBytes, context.Message.ChatId), context.CancellationToken);
                }
            }
        }
    }
}
