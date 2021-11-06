using System;
using System.Threading.Tasks;

namespace DistributedTransactions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DistributedTransactionAttribute : Attribute
    {
        public string TransactionType { get; }

        public string OperationType { get; }

        public Type RollbackDataType { get; }

        public int? OperationPriority { get; }

        public DistributedTransactionAttribute(string transactionType, string operationType, Type rollbackDataType)
        {
            TransactionType = transactionType;
            OperationType = operationType;
            RollbackDataType = rollbackDataType;
        }

        public DistributedTransactionAttribute(string transactionType, string operationType, Type rollbackDataType, int operationPriority)
        {
            TransactionType = transactionType;
            OperationType = operationType;
            RollbackDataType = rollbackDataType;
            OperationPriority = operationPriority;
        }
    }
}
