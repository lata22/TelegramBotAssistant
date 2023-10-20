using System.ComponentModel.DataAnnotations;

namespace CognitiveServices.Db.Entities
{
    public class DocumentIngestion
    {
        [Key]
        public int Id { get; set; }
        public long TelegramChatid { get;set; }
        public string FileName { get; set; } = default!;
        public string DocumentId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
