using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using TelegramBot.Interfaces;

namespace TelegramBot
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITelegramBotUpdateHandler _telegramBotUpdateHandler;
        private readonly ITelegramBotCommandUpdaterHandler _telegramBotCommandUpdaterHandler;
        private readonly ITelegramBotOnApiResponseHandler _telegramBotOnApiResponseHandler;
        private readonly ITelegramBotErrorHandler _telegramBotErrorHandler;
        private readonly ILogger<TelegramBotService> _logger;
        public TelegramBotService(
            ITelegramBotClient botClient,
            ITelegramBotCommandUpdaterHandler telegramBotCommandUpdaterHandler,
            ITelegramBotOnApiResponseHandler telegramBotOnApiResponseHandler,
            ITelegramBotUpdateHandler telegramBotUpdateHandler,
            ITelegramBotErrorHandler telegramBotErrorHandler,
            ILogger<TelegramBotService> logger)
        {
            _botClient = botClient;
            _telegramBotCommandUpdaterHandler = telegramBotCommandUpdaterHandler;
            _telegramBotOnApiResponseHandler = telegramBotOnApiResponseHandler;
            _telegramBotUpdateHandler = telegramBotUpdateHandler;
            _telegramBotErrorHandler = telegramBotErrorHandler;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _botClient.OnApiResponseReceived += _telegramBotOnApiResponseHandler.HandleApiResponse;

            _botClient.StartReceiving(
                _telegramBotUpdateHandler.HandleUpdateAsync,
                _telegramBotErrorHandler.HandleErrorAsync,
                new ReceiverOptions(),
                cancellationToken);

            _logger.LogInformation($"{await _botClient.GetMeAsync(cancellationToken)} is running");

            await _telegramBotCommandUpdaterHandler.UpdateCommands(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _botClient.CloseAsync(cancellationToken);
        }
    }
}
