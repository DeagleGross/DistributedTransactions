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
        /// Type of transaction as an int value.
        /// I.e. there could be such transaction_types: `create model1 and model2 in services`.
        /// It is just a unique description for recognizing specific type of transaction
        /// It is recommended to pass an enum type here, so that it is easy not to mess up with random values
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Current status of transaction
        /// </summary>
        public string Status { get; set; }
    }
}
