using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VD.TelegramBot.Db.Entities;

namespace VD.TelegramBot.Db.Interfaces
{
    public interface IDbContext
    {
        //DbSet<TEntity> Set<TEntity>() where TEntity : class;
        public DbSet<TelegramUser> TelegramUsers { get; set; }

        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;
    }
}
