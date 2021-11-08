using System;

namespace DistributedTransactions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DistributedTransactionOperationAttribute : Attribute
    {
        public string TransactionType { get; }

        public string OperationType { get; }

        public int? OperationPriority { get; }

        public DistributedTransactionOperationAttribute(string transactionType, string operationType)
        {
            TransactionType = transactionType;
            OperationType = operationType;
        }

        public DistributedTransactionOperationAttribute(string transactionType, string operationType, int operationPriority)
        {
            TransactionType = transactionType;
            OperationType = operationType;
            OperationPriority = operationPriority;
        }
    }
}
