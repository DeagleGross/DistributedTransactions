namespace DistributedTransactions.Providers.Abstractions
{
    public interface ITransactionContext
    {
        internal bool IsFinishedPrematurely { get; }

        /// <summary>
        /// Resolves a known service (usually it is registered in DI)
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <returns></returns>
        T GetRequiredService<T>();

        /// <summary>
        /// Saves data in memory as a string-object representation
        /// for sharing data between different commit operations in a single transaction
        /// </summary>
        /// <param name="key">unique string key for that data</param>
        /// <param name="data">data as an object</param>
        void SaveInterTransactionalData(string key, object data);

        /// <summary>
        /// Retrieves data from inter-transactional-data storage of <see cref="ITransactionContext"/>>.
        /// It makes a cast to `T` type. If a cast fails, a special exception is thrown.
        /// </summary>
        /// <param name="key">key of saved data object</param>
        /// <typeparam name="T">type to which data is casted</typeparam>
        /// <returns></returns>
        T GetInterTransactionalData<T>(string key);

        /// <summary>
        /// Marks transaction as finished prematurely
        /// That doesn't mean that transaction has failed to execute, it is just a explicit user call to stop committing further operations.
        /// </summary>
        void FinishPrematurely();

        /// <summary>
        /// Clears inter-transactional data.
        /// Not recommended to use this method, because you can lose all the data saved on previous operations in a transaction
        /// </summary>
        void ClearInterTransactionalData();
    }
}
