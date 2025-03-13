using FinTransProcessing;
using Microsoft.EntityFrameworkCore;

namespace FinTransEF
{
    public class FinTransDbContext(DbContextOptions<FinTransDbContext> options) : DbContext(options)
    {
        public static string DbSchema = "finance";
        public static string MigrationHistory = "__FinanceMigrationHistory";

        public DbSet<TransactionData> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.HasDefaultSchema(DbSchema);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinTransDbContext).Assembly);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // todo: do it only for local development
            optionsBuilder.LogTo(Console.WriteLine);
        }
    }
}
