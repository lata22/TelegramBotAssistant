using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace CognitiveServices.Db.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetSection("Data").GetSection("ConnectionString").Value;
            if (connectionString == null)
            {
                throw new ArgumentNullException($"{nameof(connectionString)} cannot be null");
            }
            services.AddDbContext<ApplicationDBContext>(optionsBuilder =>
                optionsBuilder.UseNpgsql(
                        connectionString,
                        optionsAction =>
                        {
                            optionsAction.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null);
                            optionsAction.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 0, TimeSpan.Zero, null));
                        }),
                ServiceLifetime.Scoped);

            //services.AddTransient<IDbContext, ApplicationDBContext>();
            return services;
        }
    }
}
