using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CognitiveServices.Db
{
    public class DesignTimeApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
            var connectionString = "";

            optionsBuilder.UseNpgsql(connectionString,
                    optionsAction => optionsAction.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null));

            return new ApplicationDBContext(optionsBuilder.Options);
        }
    }
}
