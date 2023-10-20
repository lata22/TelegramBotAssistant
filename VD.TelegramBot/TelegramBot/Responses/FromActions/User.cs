using Newtonsoft.Json;

namespace TelegramBot.Responses.FromActions
{
    public class User
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; } = default!;

        [JsonProperty("username")]
        public string Username { get; set; } = default!;
    }
}
