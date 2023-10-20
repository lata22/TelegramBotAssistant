using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.MessageHandlers
{
    public class FaceDetectionProcessedMessageHandler : BaseMessageHandler<FaceDetectionProcessed>
    {
        private readonly IMediator _mediator;
        private readonly ICacheReaderService<byte[]> _cacheReaderService;
        public FaceDetectionProcessedMessageHandler(
            IMediator mediator,
            ICacheReaderService<byte[]> cacheReaderService)
        {
            _mediator = mediator;
            _cacheReaderService = cacheReaderService;
        }

        public override async Task Consume(ConsumeContext<FaceDetectionProcessed> context)
        {
            if (!string.IsNullOrWhiteSpace(context.Message.CacheKey))
            {
                var image = await _cacheReaderService.GetCachedValueAsync(context.Message.CacheKey);
                if (image != null)
                {
                    await _mediator.Send(new TelegramBotImageNotificationCommand(image, context.Message.ChatId), context.CancellationToken);
                }
            }
        }
    }
}
