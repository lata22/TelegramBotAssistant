using Newtonsoft.Json;

namespace TelegramBot.Responses
{
    public class From
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; } = default!;

        [JsonProperty("username")]
        public string Username { get; set; } = default!;

        [JsonProperty("language_code")]
        public string LanguageCode { get; set; } = default!;
    }
}
