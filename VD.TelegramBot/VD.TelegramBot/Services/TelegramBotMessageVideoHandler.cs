using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotMessageVideoHandler : ITelegramBotVideoMessageHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ICacheWriterService<byte[]> _cacheWriterService;
        public TelegramBotMessageVideoHandler(
            ITelegramBotClient telegramBotClient,
            ICacheWriterService<byte[]> cacheWriterService,
            IPublishEndpoint publishEndpoint)
        {
            _telegramBotClient = telegramBotClient;
            _cacheWriterService = cacheWriterService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> HandleVideoMessage(Message message, CancellationToken cancellationToken)
        {
            var file = await _telegramBotClient.GetFileAsync(message.Video!.FileId, cancellationToken);
            if (file != null && !string.IsNullOrEmpty(file.FilePath))
            {
                await using var memoryStream = new MemoryStream();
                await _telegramBotClient.DownloadFileAsync(file.FilePath, memoryStream, cancellationToken);
                var cacheKey = Guid.NewGuid().ToString();
                await _cacheWriterService.SetCacheValueAsync(cacheKey, memoryStream.ToArray());
                await _publishEndpoint.Publish(new FaceDetectionVideoRequested(cacheKey, message.Chat.Id), cancellationToken);
            }
            return Unit.Value;
        }
    }
}
