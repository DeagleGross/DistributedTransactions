using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Models
{
    internal class DistributedTranscationOperationWrapper
    {
        public IDistributedTransactionOperation Operation { get; }

        public DistributedTranscationOperationInfo OperationInfo { get; }

        public DistributedTranscationOperationWrapper(
            IDistributedTransactionOperation operation,
            DistributedTranscationOperationInfo info)
        {
            Operation = operation;
            OperationInfo = info;
        }
    }
}
