namespace TelegramBot.Config
{
    public class TelegramBotConfig
    {
        public string CommandExecutedMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string StartMessage { get; set; } = string.Empty;
        public string[] CommandList { get; set; } = default!;
        public string[] AllowedUsernames { get; set; } = default!;
    }
}
