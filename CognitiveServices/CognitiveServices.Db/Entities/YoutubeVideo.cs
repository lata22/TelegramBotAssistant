using System.ComponentModel.DataAnnotations;

namespace CognitiveServices.Db.Entities
{
    public class YoutubeVideo
    {
        [Key]
        public int Id { get; set; }
        public string DocumentId {  get; set; } = default!;
        public string Url { get; set; } = default!;
        public long TelegramChatId { get; set; }
        public string Title { get; set; } = default!;
        public string ChannelName { get; set; } = default!;
        public string Transcription { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
