namespace FinTransProcessing
{
    internal record ParsedData
    {
        public string TransactionId { get; init; }
        public string UserId { get; init; }
        public DateTime Date { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public string Merchant { get; init; }
    }

}
