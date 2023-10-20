using Newtonsoft.Json;

namespace TelegramBot.Responses.FromCommands
{
    public class Chat
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; } = default!;

        [JsonProperty("username")]
        public string Username { get; set; } = default!;

        [JsonProperty("type")]
        public string Type { get; set; } = default!;
    }
}
