using System.ComponentModel.DataAnnotations;

namespace CognitiveServices.Db.Entities
{
    public class ChatSession
    {
        [Key]
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChatMessage> Messages { get; set; } = default!;
    }
}
