namespace DistributedTransactions
{
    public enum TransactionStatus
    {
        // successful flow
        Created,
        FinishedCorrectly,

        // unsuccessful flow
        NeedsToRollback,
        FinishedWithRollback
    }
}
