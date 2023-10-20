using Telegram.Bot;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{

    public class TelegramBotErrorHandler : ITelegramBotErrorHandler
    {
        private readonly ILogger<TelegramBotErrorHandler> _logger;
        public TelegramBotErrorHandler(ILogger<TelegramBotErrorHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, exception.Message);
            await Task.CompletedTask;
        }
    }
}
