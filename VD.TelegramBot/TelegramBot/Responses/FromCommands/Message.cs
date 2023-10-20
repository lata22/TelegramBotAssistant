using Newtonsoft.Json;

namespace TelegramBot.Responses.FromCommands
{
    public class Message
    {
        [JsonProperty("message_id")]
        public int MessageId { get; set; }

        [JsonProperty("from")]
        public From From { get; set; } = default!;

        [JsonProperty("chat")]
        public Chat Chat { get; set; } = default!;

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; } = default!;

        [JsonProperty("entities")]
        public Entity[] Entities { get; set; } = default!;
    }
}
