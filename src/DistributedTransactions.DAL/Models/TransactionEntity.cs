namespace DistributedTransactions.DAL.Models
{
    /// <summary>
    /// Represents a status of a transaction.
    /// Every single transaction, that is executed by service is saved as a unique transaction
    /// </summary>
    public class TransactionEntity
    {
        /// <summary>
        /// Unique id of a transaction
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Id of transaction group.
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// Current status of transaction
        /// </summary>
        public string Status { get; set; }
    }
}
