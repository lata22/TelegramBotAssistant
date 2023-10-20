namespace TelegramBot.Interfaces
{
    public interface ITelegramBotCommandUpdaterHandler : ITelegramBotSingleton
    {
        public Task UpdateCommands(CancellationToken cancellationToken);
    }
}
