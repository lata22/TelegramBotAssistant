using CognitiveServices.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CognitiveServices.Db.Interfaces
{
    public interface IDbContext
    {
        //DbSet<TEntity> Set<TEntity>() where TEntity : class;
        public DbSet<AudioTranscription> AudioTranscriptions { get; set; }
        public DbSet<YoutubeVideo> YoutubeTranscriptions { get; set; } 
        public DbSet<ChatSession> ChatSessions { get; set; }

        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;
    }
}
