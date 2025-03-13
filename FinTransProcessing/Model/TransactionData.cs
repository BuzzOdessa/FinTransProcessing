using System.ComponentModel.DataAnnotations.Schema;

namespace FinTransProcessing.Model
{
    public record TransactionData
    {
        public string TransactionId { get; init; }
        public string UserId { get; init; }
        [Column(TypeName = "timestamp(6)")]
        public DateTime Date { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public string Merchant { get; init; }
    }

}
