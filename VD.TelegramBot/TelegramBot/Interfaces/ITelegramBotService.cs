namespace TelegramBot.Interfaces
{
    public interface ITelegramBotService : ITelegramBotSingleton
    {
        Task InitializeAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
