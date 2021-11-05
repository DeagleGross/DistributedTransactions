using System;

namespace DistributedTransactions.Models
{
    public class DistributedTransactionOperationState<T>
    {
        public long? OperationId { get; set; }

        public long TransactionGroupId { get; set; }

        public int? OperationPriority { get; set; }

        public Type RollbackDataType { get; set; }

        public T RollbackData { get; set; }

        public OperationState State { get; set; }
    }
}
