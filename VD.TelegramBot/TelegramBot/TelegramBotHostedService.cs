using Microsoft.Extensions.Hosting;
using TelegramBot.Interfaces;

namespace TelegramBot
{
    public class TelegramBotHostedService : IHostedService
    {
        private readonly ITelegramBotService _telegramBotService;

        public TelegramBotHostedService(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _telegramBotService.InitializeAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _telegramBotService.StopAsync(cancellationToken);
        }
    }
}
