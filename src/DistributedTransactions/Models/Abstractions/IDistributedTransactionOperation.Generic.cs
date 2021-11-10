namespace DistributedTransactions.Models.Abstractions
{
    public interface IDistributedTransactionOperation<TRollbackData> : IDistributedTransactionOperationExecutor
    {
        TRollbackData RollbackData { get; set; }
    }
}
