namespace TelegramBot.Interfaces
{
    public interface ITelegramBotCommandTrigger : ITelegramBotSingleton
    {
        Task TriggerCommand(string command, CancellationToken cancellationToken);
    }
}
