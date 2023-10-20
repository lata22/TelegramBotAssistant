using System.ComponentModel.DataAnnotations;

namespace CognitiveServices.Db.Entities
{
    public class AudioTranscription
    {
        [Key]
        public int Id { get; set; }
        public string DocumentId { get; set; } = default!;
        public string? FileName { get; set; }
        public long TelegramChatId { get; set; }
        public string Transcription { get; set; } = default!;
        public string? Summary { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
