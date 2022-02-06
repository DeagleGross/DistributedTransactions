namespace DistributedTransactions.Models.Settings
{
    /// <summary>
    /// Settings specifying options of distributed_executors - retry policy, service owner info.
    /// </summary>
    public class DistributedTransactionSettings
    {
        /// <summary>
        /// Policy for retry that can be turned on\off and retry count for different operation types could be defined
        /// </summary>
        public RetryPolicy RetryPolicy { get; internal set; } = new()
        {
            IsTurnedOn = false,
            CommitRetryCount = 3,
            RollbackRetryCount = 3
        };

        /// <summary>
        /// Information about service that is hosting a distributed transaction
        /// </summary>
        public DistributedTransactionServiceOwnerInfo ServiceOwnerInfo { get; set; }

        public DistributedTransactionSettings(DistributedTransactionServiceOwnerInfo serviceOwnerInfo = null, RetryPolicy retryPolicy = null)
        {
            ServiceOwnerInfo = serviceOwnerInfo;
            if (retryPolicy is not null) RetryPolicy = retryPolicy;
        }
    }
}
