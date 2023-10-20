using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotDocumentMessageHandler : ITelegramBotDocumentMessageHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ICacheWriterService<byte[]> _cacheWriterService;
        private readonly ILogger<TelegramBotDocumentMessageHandler> _logger;
        public TelegramBotDocumentMessageHandler(
            ITelegramBotClient telegramBotClient,
            ICacheWriterService<byte[]> cacheWriterService,
            IPublishEndpoint publishEndpoint,
            ILogger<TelegramBotDocumentMessageHandler> logger)
        {
            _telegramBotClient = telegramBotClient;
            _cacheWriterService = cacheWriterService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<Unit> HandleDocumentMessage(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var document = await _telegramBotClient.GetFileAsync(message.Document!.FileId, cancellationToken);
                if (document != null && !string.IsNullOrEmpty(document.FilePath))
                {
                    using var memoryStream = new MemoryStream();
                    string fileName = Path.GetFileName(document.FilePath);
                    await _telegramBotClient.DownloadFileAsync(document.FilePath, memoryStream, cancellationToken);
                    await _cacheWriterService.SetCacheValueAsync(document.FileId, memoryStream.ToArray());
                    await Task.WhenAll(
                        _publishEndpoint.Publish(new DocumentCreated(document.FileId, fileName, message.Chat.Id), cancellationToken),
                        _publishEndpoint.Publish(new DocumentSemanticIngestionRequested(document.FileId, message.Chat.Id, fileName)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return Unit.Value;
        }
    }
}
