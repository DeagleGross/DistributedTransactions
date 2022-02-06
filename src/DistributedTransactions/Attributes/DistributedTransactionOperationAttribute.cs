using System;

namespace DistributedTransactions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DistributedTransactionOperationAttribute : Attribute
    {
        /// <summary>
        /// Name of transaction type
        /// </summary>
        public string TransactionType { get; }

        /// <summary>
        /// Name of operation type specified inside a transaction
        /// </summary>
        public string OperationType { get; }

        /// <summary>
        /// Priority of rollback. The more, the later operation will be rollbacked.
        /// Default value is 0, so don't pass it explicitly.
        /// </summary>
        public int RollbackPriority { get; set; }

        /// Specifies stage among other operations when this operation will be executed.
        /// Default value is 0, so don't pass it explicitly.
        public int ExecutionStage { get; set; }

        public DistributedTransactionOperationAttribute(string transactionType, string operationType)
        {
            TransactionType = transactionType;
            OperationType = operationType;
        }
    }
}
