using Newtonsoft.Json;

namespace TelegramBot.Responses.FromActions
{
    public class New_Chat_Member
    {
        [JsonProperty("user")]
        public User User { get; set; } = default!;


        [JsonProperty("status")]
        public string Status { get; set; } = default!;


        [JsonProperty("until_date")]
        public int Until_date { get; set; }
    }
}
