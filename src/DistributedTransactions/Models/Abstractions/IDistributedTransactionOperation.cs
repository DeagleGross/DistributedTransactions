namespace DistributedTransactions.Models.Abstractions
{
    internal interface IDistributedTransactionOperation : IDistributedTransactionOperationExecutor
    {
        public object RollbackData { get; set; }
    }
}
