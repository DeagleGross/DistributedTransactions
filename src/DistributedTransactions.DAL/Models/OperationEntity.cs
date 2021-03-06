namespace DistributedTransactions.DAL.Models
{
    /// <summary>
    /// Every single operation executed as a part of transaction by service is saved for possible rollbacks.
    /// </summary>
    public class OperationEntity
    {
        /// <summary>
        /// Unique id of transaction operation
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Id of existing transaction, in which operation is executed
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        /// Type of operation in a transaction.
        /// I.e. there could be operation_types: `updateModel`, `createModel` and `deleteModel` in the same transaction.
        /// It is recommended to pass an enum type here, so that it is easy not to mess up with random values
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Priority of operation when rollback of transaction is executed.
        /// </summary>
        public int? RollbackPriority { get; set; }

        /// <summary>
        /// Stage of execution. Allows different operations with same `ExecutionStage` to execute in parallel.
        /// The more execution stage value is, the latter it will be executed.
        /// </summary>
        public int? ExecutionStage { get; set; }

        /// <summary>
        /// System.Type of rollback_data_type saved for rollback_data for further deserialization
        /// represented as a System.String type.
        /// </summary>
        public string RollbackDataType { get; set; }

        /// <summary>
        /// System.Type of a class, that implements IDistributedTransactionOperationExecutor.
        /// It is needed to use reflection to instantiate and get an object, from which we can invoke rollback method
        /// </summary>
        public string ExecutorType { get; set; }

        /// <summary>
        /// rollback_data serialized as a string
        /// </summary>
        public string RollbackData { get; set; }

        /// <summary>
        /// Current status of transaction operation. Shows if it is just saved, needs to be rollbacked, already rollbacked and etc.
        /// </summary>
        public string Status { get; set; }
    }
}
