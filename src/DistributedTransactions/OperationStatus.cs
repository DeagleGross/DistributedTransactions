namespace DistributedTransactions
{
    public enum OperationStatus
    {
        /// <summary>
        /// Successfully first try committed
        /// </summary>
        Committed,

        /// <summary>
        /// Describes an operation that has failed to commit.
        /// </summary>
        NeedsToRollback,

        /// <summary>
        /// Describes an operation that has failed to rollback.
        /// Worker will still try to rollback such operations, but it doesn't mean it will ever succeed.
        /// </summary>
        FailedToRollback,

        /// <summary>
        /// Succeeded to rollback from <see cref="NeedsToRollback"/> or <see cref="FailedToRollback"/> statuses
        /// </summary>
        Rollbacked
    }
}
