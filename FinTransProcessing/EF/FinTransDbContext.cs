using System;
using System.Data.Common;
using System.Drawing;
using FinTransProcessing.Model;
using Microsoft.EntityFrameworkCore;

namespace FinTransProcessing.EF
{
    //https://metanit.com/sharp/efcore/1.2.php
    public class FinTransDbContext(string connectionString) : DbContext
    {
        public static string DbSchema = "finance";
        public static string MigrationHistory = "__FinanceMigrationHistory";

        public bool makeLog = true;
        public bool logDbCommandOnly = true;
        public DbSet<TransactionData> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.HasDefaultSchema(DbSchema);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinTransDbContext).Assembly);

        }

        void doLog(string msg)
        {

            if (makeLog)
            {
                bool log = !logDbCommandOnly || msg.Contains("Executing DbCommand");
                if (log)
                    Console.WriteLine(msg);
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // todo: do it only for local development
            //    optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.LogTo(doLog);

            // todo: Вычитать коннекшн стринг из конфига
            // optionsBuilder.UseNpgsql("Host=localhost;Port=54320;Database=finance;Username=buzz;Password=buzz");
            optionsBuilder.UseNpgsql(connectionString);
            
        }
    }
}
