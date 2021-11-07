namespace DistributedTransactions.Models
{
    public class Transaction
    {
        public long Id { get; set; }

        public string TransactionType { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
