using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTransEF
{
    public static class FinTransDbContextRegistration
    {
        public static void RegisterLibraryDbContext(this IServiceCollection services, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("LibraryDb");

            services.AddDbContext<FinTransDbContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable(
                            FinTransDbContext.MigrationHistory,
                            FinTransDbContext.DbSchema);
                    });

            });
        }
    }
}
