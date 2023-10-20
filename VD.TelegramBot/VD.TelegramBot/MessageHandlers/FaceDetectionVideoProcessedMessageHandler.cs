using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.MessageHandlers
{
    public class FaceDetectionVideoProcessedMessageHandler : BaseMessageHandler<FaceDetectionVideoProcessed>
    {
        private readonly IMediator _mediator;
        private readonly ICacheReaderService<byte[]> _cacheReaderService;
        public FaceDetectionVideoProcessedMessageHandler(
            IMediator mediator,
            ICacheReaderService<byte[]> cacheReaderService)
        {
            _mediator = mediator;
            _cacheReaderService = cacheReaderService;
        }

        public override async Task Consume(ConsumeContext<FaceDetectionVideoProcessed> context)
        {
            if (!string.IsNullOrWhiteSpace(context.Message.CacheKey))
            {
                var video = await _cacheReaderService.GetCachedValueAsync(context.Message.CacheKey);
                if (video != null)
                {
                    await _mediator.Send(new TelegramBotVideoNotificationCommand(video, context.Message.ChatId), context.CancellationToken);
                }
            }
        }
    }
}
