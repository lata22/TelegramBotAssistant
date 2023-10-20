using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotMessageAudioHandler : ITelegramBotAudioMessageHandler
    {
        private const int TELEGRAM_MAX_FILE_SIZE = 20 * 1024 * 1024;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ICacheWriterService<byte[]> _cacheWriterService;
        private readonly ILogger<TelegramBotMessageAudioHandler> _logger;
        public TelegramBotMessageAudioHandler(
            ITelegramBotClient telegramBotClient,
            ICacheWriterService<byte[]> cacheWriterService,
            IPublishEndpoint publishEndpoint,
            ILogger<TelegramBotMessageAudioHandler> logger)
        {
            _telegramBotClient = telegramBotClient;
            _cacheWriterService = cacheWriterService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<Unit> HandleAudioMessage(Message message, CancellationToken cancellationToken)
        {
            var file = await _telegramBotClient.GetFileAsync(message.Voice!.FileId, cancellationToken);

            if (file != null && !string.IsNullOrEmpty(file.FilePath) && file.FileSize < TELEGRAM_MAX_FILE_SIZE)
            {
                await using var memoryStream = new MemoryStream();
                try
                {
                    await _telegramBotClient.DownloadFileAsync(file.FilePath, memoryStream, cancellationToken);
                    await _cacheWriterService.SetCacheValueAsync(file.FileId, memoryStream.ToArray());
                    await _publishEndpoint.Publish(new AudioTranscriptionRequested(file.FileId, message.Chat.Id), cancellationToken);
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
