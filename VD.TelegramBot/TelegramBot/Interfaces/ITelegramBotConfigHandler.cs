using TelegramBot.Config;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotConfigHandler : ITelegramBotSingleton
    {
        public Task<TelegramBotConfig> ReadConfig(CancellationToken cancellationToken, string jsonFilePath = "");
        public Task UpdateConfig(TelegramBotConfig newConfig, CancellationToken cancellationToken, string jsonFilePath = "");
    }
}
