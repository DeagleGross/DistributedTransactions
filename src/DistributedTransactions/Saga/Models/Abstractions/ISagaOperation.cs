namespace DistributedTransactions.Saga.Models.Abstractions
{
    internal interface ISagaOperation : ISagaOperationExecutor
    {
        public object RollbackData { get; set; }
    }
}
