using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VD.TelegramBot.Db.Interfaces;

namespace VD.TelegramBot.Db.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetSection("Data").GetSection("ConnectionString").Value;
            if (connectionString == null)
            {
                throw new ArgumentNullException($"{nameof(connectionString)} in appsettings.json cannot be null");
            }
            services.AddDbContext<ApplicationDBContext>(optionsBuilder =>
                optionsBuilder.UseNpgsql(
                        connectionString,
                        optionsAction =>
                            optionsAction.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null)),
                ServiceLifetime.Transient);
            services.AddTransient<IDbContext, ApplicationDBContext>();
            return services;
        }
    }
}
