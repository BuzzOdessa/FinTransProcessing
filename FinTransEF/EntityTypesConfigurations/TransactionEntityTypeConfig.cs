using Microsoft.EntityFrameworkCore;
using FinTransProcessing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTransEF.EntityTypesConfigurations
{
    public class TransactionEntityTypeConfig : IEntityTypeConfiguration<TransactionData>
    {
        public void Configure(EntityTypeBuilder<TransactionData> builder)
        {
            builder.HasKey(x => x.TransactionId);

            builder.Property(x => x.TransactionId)
                        .HasMaxLength(38)
                        .IsRequired();
            builder.Property(x => x.UserId)
                        .HasMaxLength(38)
                        .IsRequired();
            builder.Property(x => x.Category)
                        .HasMaxLength(200);
            builder.Property(x => x.Description)
                        .HasMaxLength(200);
            builder.Property(x => x.Merchant)
                        .HasMaxLength(200);

            /*public string UserId { get; init; }             * 
                  public DateTime Date { get; init; }
                  public decimal Amount { get; init; }
                  public string Category { get; init; }
                  public string Description { get; init; }
                  public string Merchant { get; init; }*/
        }
    }
}
