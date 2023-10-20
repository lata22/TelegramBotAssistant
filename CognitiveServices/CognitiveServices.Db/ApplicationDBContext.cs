using CognitiveServices.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace CognitiveServices.Db
{
    public class ApplicationDBContext : DbContext
    {
        public DbSet<YoutubeVideoIngestionState> YoutubeVideoIngestionStates { get; set; }
        public DbSet<AudioTranscription> AudioTranscriptions { get; set; } = default!;
        public DbSet<YoutubeVideo> YoutubeVideos { get; set; } = default!;
        public DbSet<ChatSession> ChatSessions { get; set; } = default!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
        public DbSet<DocumentIngestion> DocumentIngestions { get; set; } = default!;
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected ApplicationDBContext(DbContextOptions contextOptions) : base(contextOptions)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>()
                .HasOne(p => p.Session)
                .WithMany(b => b.Messages)
                .HasForeignKey(p => p.ChatSessionId);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Error);

    }
}
