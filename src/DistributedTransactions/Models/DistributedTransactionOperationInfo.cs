using DistributedTransactions.Attributes;
using System;

namespace DistributedTransactions.Models
{
    public record DistributedTranscationOperationInfo
    {
        public long OperationId { get; }

        public long TransactionGroupId { get; }

        public int? OperatorPriority { get; }

        public Type RollbackDataType { get; }

        public DistributedTranscationOperationInfo(DistributedTransactionAttribute attribute)
        {
            OperationId = attribute.OperationId;
            TransactionGroupId = attribute.TransactionGroupId;
            OperatorPriority = attribute.OperationPriority;
            RollbackDataType = attribute.RollbackDataType;
        }

        public override string ToString() => $"OperationInfo: OperationId='{OperationId}', TransactionGroupId='{TransactionGroupId}', OperatorPriority='{OperatorPriority}'";
    }
}
