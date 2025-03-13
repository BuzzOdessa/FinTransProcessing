using System;
using System.Drawing;
using FinTransProcessing.Model;
using Microsoft.EntityFrameworkCore;

namespace FinTransProcessing.EF
{
    //https://metanit.com/sharp/efcore/1.2.php
    public class FinTransDbContext : DbContext
    {
        public static string DbSchema = "finance";
        public static string MigrationHistory = "__FinanceMigrationHistory";

        public bool makeLog = true;
        public DbSet<TransactionData> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.HasDefaultSchema(DbSchema);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinTransDbContext).Assembly);

        }

        void doLog(string msg)
        {
            if (makeLog)
                Console.WriteLine(msg);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // todo: do it only for local development
            //    optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.LogTo(doLog);

            optionsBuilder.UseNpgsql("Host=localhost;Port=54320;Database=finance;Username=buzz;Password=buzz");
        }
    }
}
