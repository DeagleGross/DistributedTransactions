namespace DistributedTransactions.Models.Abstractions
{
    public interface IDistributedTransactionOperation<T> : IDistributedTransactionOperationExecutor
    {
        T RollbackData { get; set; }
    }
}
