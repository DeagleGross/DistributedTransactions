using DistributedTransactions.Models;
using DistributedTransactions.Saga.Models.Abstractions;

namespace DistributedTransactions.Saga.Models
{
    internal class SagaOperationExecutorWithInfo
    {
        public ISagaOperation OperationExecutor { get; }

        public OperationInfo OperationInfo { get; }

        public SagaOperationExecutorWithInfo(
            ISagaOperation operationExecutor,
            OperationInfo info)
        {
            OperationExecutor = operationExecutor;
            OperationInfo = info;
        }
    }
}
