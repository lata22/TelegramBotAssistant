namespace VD.TelegramBot.Db.Entities
{
    public class TelegramUser
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
