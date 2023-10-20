using Newtonsoft.Json;

namespace TelegramBot.Responses.FromCommands
{
    public class Result
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; } = default!;
    }
}
