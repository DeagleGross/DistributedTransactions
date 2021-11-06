using DistributedTransactions.Attributes;
using System;

namespace DistributedTransactions.Models
{
    public record DistributedTransactionOperationInfo
    {
        public string TransactionType { get; }

        public string OperationType { get; }

        public Type RollbackDataType { get; }

        public int? OperationPriority { get; }

        public DistributedTransactionOperationInfo(DistributedTransactionAttribute attribute)
        {
            TransactionType = attribute.TransactionType;
            OperationType = attribute.OperationType;
            RollbackDataType = attribute.RollbackDataType;
            OperationPriority = attribute.OperationPriority;
        }

        public override string ToString() => $"OperationInfo: OperationType='{OperationType}', TransactionType='{TransactionType}', OperationPriority='{OperationPriority}'";
    }
}
