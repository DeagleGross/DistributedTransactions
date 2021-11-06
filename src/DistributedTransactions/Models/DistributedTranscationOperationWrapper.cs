using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Models
{
    internal class DistributedTranscationOperationWrapper
    {
        public IDistributedTransactionOperation Operation { get; }

        public DistributedTransactionOperationInfo OperationInfo { get; }

        public DistributedTranscationOperationWrapper(
            IDistributedTransactionOperation operation,
            DistributedTransactionOperationInfo info)
        {
            Operation = operation;
            OperationInfo = info;
        }
    }
}
