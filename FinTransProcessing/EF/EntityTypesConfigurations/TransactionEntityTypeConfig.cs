using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTransProcessing.Model;

namespace FinTransProcessing.EF.EntityTypesConfigurations
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
        }
    }
}
