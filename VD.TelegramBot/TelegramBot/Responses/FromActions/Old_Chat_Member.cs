using Newtonsoft.Json;

namespace TelegramBot.Responses.FromActions
{
    public class Old_Chat_Member
    {
        [JsonProperty("user")]
        public User User { get; set; } = default!;

        [JsonProperty("status")]
        public string Status { get; set; } = default!;
    }
}
