using Newtonsoft.Json;

namespace TelegramBot.Responses.FromCommands
{
    public class TelegramResponseFromCommand
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("result")]
        public Result[] Result { get; set; } = default!;
    }
}
