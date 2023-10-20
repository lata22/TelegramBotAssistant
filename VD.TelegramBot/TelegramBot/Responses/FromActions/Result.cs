using Newtonsoft.Json;

namespace TelegramBot.Responses.FromActions
{
    public class Result
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }

        [JsonProperty("my_chat_member")]
        public My_Chat_Member MyChatMember { get; set; } = default!;
    }
}
