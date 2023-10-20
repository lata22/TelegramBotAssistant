using Newtonsoft.Json;

namespace TelegramBot.Responses.FromActions
{
    public class TelegramResponseFromAction
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("result")]
        public Result[] Result { get; set; } = default!;
    }
}
