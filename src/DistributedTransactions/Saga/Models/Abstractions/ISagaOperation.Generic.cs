namespace DistributedTransactions.Saga.Models.Abstractions
{
    public interface ISagaOperation<TRollbackData> : ISagaOperationExecutor
    {
        TRollbackData RollbackData { get; set; }
    }
}
