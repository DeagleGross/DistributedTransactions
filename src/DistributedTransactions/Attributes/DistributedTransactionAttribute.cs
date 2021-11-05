using System;

namespace DistributedTransactions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DistributedTransactionAttribute : Attribute
    {
        public long OperationId { get; set; }

        public long TransactionGroupId { get; }

        public int? OperationPriority { get; }

        public Type RollbackDataType { get; }

        public DistributedTransactionAttribute(long operationId, long transactionGroupId, Type rollbackDataType, int? operationPriority = null)
        {
            OperationId = operationId;
            TransactionGroupId = transactionGroupId;
            RollbackDataType = rollbackDataType;
            OperationPriority = operationPriority;
        }
    }
}
