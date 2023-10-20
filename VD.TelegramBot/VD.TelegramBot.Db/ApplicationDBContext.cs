using VD.TelegramBot.Db.Entities;
using VD.TelegramBot.Db.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace VD.TelegramBot.Db
{
    public class ApplicationDBContext : DbContext, IDbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; } = default!;

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected ApplicationDBContext(DbContextOptions contextOptions) : base(contextOptions)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.LogTo(Console.WriteLine);

    }
}
