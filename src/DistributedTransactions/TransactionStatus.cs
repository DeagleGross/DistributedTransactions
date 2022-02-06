namespace DistributedTransactions
{
    public enum TransactionStatus
    {
        #region Success

        /// <summary>
        /// Just initially created transaction.
        /// Didn't execute any operations at this moment
        /// </summary>
        Created,

        /// <summary>
        /// Transaction has committed all of the operations successfully
        /// </summary>
        FinishedCorrectly,

        /// <summary>
        /// User has explicitly called to stop transaction execution.
        /// Basically it is a `success` type of status, because user has defined it by himself
        /// </summary>
        FinishedPrematurely,

        #endregion

        # region Rollbacks

        /// <summary>
        /// One of commit methods of any operation has failed.
        /// Describes a transaction that needs to be rollbacked at some moment.
        /// </summary>
        NeedsToRollback,

        /// <summary>
        /// Means that operation has started rollback flow at least once,
        /// but it has failed on any SagaOperation.Rollback method.
        ///
        /// So it maybe will rollback at some time, but maybe not,
        /// so be careful and be ready to manually rollback operations of such a transaction
        /// </summary>
        FailedToRollback,

        /// <summary>
        /// All of previously committed operations binded to transaction have successfully rollbacked
        /// </summary>
        FinishedWithRollback,

        #endregion

        #region Fails

        /// <summary>
        /// Transaction has failed on the first operation commit.
        /// This means there is nothing needed to rollback, but we need to mark transaction as <see cref="Failed"/>, not leave it as <see cref="Created"/>
        /// </summary>
        Failed

        #endregion
    }
}
