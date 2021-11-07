using System;
using DistributedTransactions.Attributes;

namespace DistributedTransactions.Models
{
    public record OperationInfo
    {
        public string TransactionType { get; }

        public string OperationType { get; }

        public Type RollbackDataType { get; }

        public int? OperationPriority { get; }

        public OperationInfo(DistributedTransactionOperationAttribute operationAttribute)
        {
            TransactionType = operationAttribute.TransactionType;
            OperationType = operationAttribute.OperationType;
            RollbackDataType = operationAttribute.RollbackDataType;
            OperationPriority = operationAttribute.OperationPriority;
        }

        public override string ToString() => $"OperationInfo: OperationType='{OperationType}', TransactionType='{TransactionType}', OperationPriority='{OperationPriority}'";
    }
}
