using System;

namespace DistributedTransactions.Models
{
    public class Operation
    {
        public long Id { get; set; }

        public long TransactionId { get; set; }

        public string OperationType { get; set; }

        public int? RollbackOperationPriority { get; set; }

        public Type ExecutorType { get; set; }

        public Type RollbackDataType { get; set; }

        public object RollbackData { get; set; }

        public OperationStatus Status { get; set; }
    }
}
