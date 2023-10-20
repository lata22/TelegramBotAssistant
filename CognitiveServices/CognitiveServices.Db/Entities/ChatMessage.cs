using System.ComponentModel.DataAnnotations;

namespace CognitiveServices.Db.Entities
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public long ChatSessionId { get; set; }
        public string AuthorRole { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public ChatSession Session { get; set; } = default!;
    }
}
