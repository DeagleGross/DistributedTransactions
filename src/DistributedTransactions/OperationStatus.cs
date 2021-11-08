namespace DistributedTransactions
{
    public enum OperationStatus
    {
        Committed,
        NeedsToRollback,
        Rollbacked
    }
}
