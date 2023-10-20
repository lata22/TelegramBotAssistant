using Infrastructure.Firebase.Storage;
using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotMessagePhotoHandler : ITelegramBotPhotoMessageHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITelegramBotFileSaver _telegramBotFileSaver;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ICacheWriterService<byte[]> _cacheWriterService;
        public TelegramBotMessagePhotoHandler(
            ITelegramBotClient telegramBotClient,
            ITelegramBotFileSaver telegramBotFileSaver,
            ICacheWriterService<byte[]> cacheWriterService,
            IPublishEndpoint publishEndpoint)
        {
            _telegramBotClient = telegramBotClient;
            _telegramBotFileSaver = telegramBotFileSaver;
            _cacheWriterService = cacheWriterService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> HandlePhotoMessage(Message message, CancellationToken cancellationToken)
        {
            var file = await _telegramBotClient.GetFileAsync(message.Photo![message.Photo.Length - 1].FileId, cancellationToken);
            if (file != null && !string.IsNullOrEmpty(file.FilePath))
            {
                await using var memoryStream = new MemoryStream();
                await _telegramBotClient.DownloadFileAsync(file.FilePath, memoryStream, cancellationToken);
                var cacheKey = Guid.NewGuid().ToString();
                await _cacheWriterService.SetCacheValueAsync(cacheKey, memoryStream.ToArray());
                await _publishEndpoint.Publish(new FaceDetectionRequested(cacheKey, message.Chat.Id), cancellationToken);
                await _telegramBotFileSaver.SaveFileAsync(memoryStream.ToArray(), message.Chat.Id, Path.GetExtension(file.FilePath), GoogleStorageFileTypes.Images, cancellationToken);
            }
            return Unit.Value;
        }
    }
}
