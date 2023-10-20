using Newtonsoft.Json;

namespace TelegramBot.Responses.FromCommands
{
    public class Entity
    {
        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = default!;
    }
}
