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

        public int? ExecutionStage { get; }

        public OperationInfo(DistributedTransactionOperationAttribute operationAttribute, Type rollbackDataType, Type executorType)
        {
            // info from attribute
            TransactionType = operationAttribute.TransactionType;
            OperationType = operationAttribute.OperationType;
            RollbackOperationPriority = operationAttribute.RollbackPriority == 0 ? null : operationAttribute.RollbackPriority;
            ExecutionStage = operationAttribute.ExecutionStage == 0 ? null : operationAttribute.ExecutionStage;

            // other info
            RollbackDataType = rollbackDataType;
            ExecutorType = executorType;
        }

        public override string ToString() => $"OperationInfo: OperationType='{OperationType}', TransactionType='{TransactionType}', RollbackPriority='{RollbackOperationPriority}', ExecutionStage='{ExecutionStage}'";
    }
}
