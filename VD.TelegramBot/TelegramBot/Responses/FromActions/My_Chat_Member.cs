
using Newtonsoft.Json;

namespace TelegramBot.Responses.FromActions
{
    public class My_Chat_Member
    {
        [JsonProperty("chat")]
        public Chat Chat { get; set; } = default!;

        [JsonProperty("from")]
        public From From { get; set; } = default!;

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("old_chat_member")]
        public Old_Chat_Member OldChatMember { get; set; } = default!;

        [JsonProperty("new_chat_member")]
        public New_Chat_Member NewChatMember { get; set; } = default!;
    }
}
