using System;
using DistributedTransactions.Attributes;

namespace DistributedTransactions.Models
{
    internal record OperationInfo
    {
        public string TransactionType { get; }

        public string OperationType { get; }

        public Type RollbackDataType { get; }

        public Type ExecutorType { get; }

        public int? RollbackOperationPriority { get; }
        
        public OperationInfo(DistributedTransactionOperationAttribute operationAttribute, Type rollbackDataType, Type executorType)
        {
            TransactionType = operationAttribute.TransactionType;
            OperationType = operationAttribute.OperationType;
            RollbackOperationPriority = operationAttribute.OperationPriority;

            RollbackDataType = rollbackDataType;
            ExecutorType = executorType;
        }

        public override string ToString() => $"OperationInfo: OperationType='{OperationType}', TransactionType='{TransactionType}', RollbackOperationPriority='{RollbackOperationPriority}'";
    }
}
