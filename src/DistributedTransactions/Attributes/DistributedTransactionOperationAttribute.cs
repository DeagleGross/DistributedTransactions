using System;

namespace DistributedTransactions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DistributedTransactionOperationAttribute : Attribute
    {
        public string TransactionType { get; }

        public string OperationType { get; }

        public Type RollbackDataType { get; }

        public int? OperationPriority { get; }

        public DistributedTransactionOperationAttribute(string transactionType, string operationType, Type rollbackDataType)
        {
            TransactionType = transactionType;
            OperationType = operationType;
            RollbackDataType = rollbackDataType;
        }

        public DistributedTransactionOperationAttribute(string transactionType, string operationType, Type rollbackDataType, int operationPriority)
        {
            TransactionType = transactionType;
            OperationType = operationType;
            RollbackDataType = rollbackDataType;
            OperationPriority = operationPriority;
        }
    }
}
