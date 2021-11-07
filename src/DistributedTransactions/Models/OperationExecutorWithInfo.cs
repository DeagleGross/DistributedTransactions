using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Models
{
    internal class OperationExecutorWithInfo
    {
        public IDistributedTransactionOperation OperationExecutor { get; }

        public OperationInfo OperationInfo { get; }

        public OperationExecutorWithInfo(
            IDistributedTransactionOperation operationExecutor,
            OperationInfo info)
        {
            OperationExecutor = operationExecutor;
            OperationInfo = info;
        }
    }
}
