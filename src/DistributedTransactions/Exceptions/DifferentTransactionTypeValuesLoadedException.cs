using System;
using DistributedTransactions.Executors;
using DistributedTransactions.Models;

namespace DistributedTransactions.Exceptions
{
    public class DifferentTransactionTypeValuesLoadedException : Exception
    {
        public DifferentTransactionTypeValuesLoadedException(string oldTransactionType, string newTransactionType)
            : base($"Can not load operations with different transaction types in a single {nameof(DistributedTransactionExecutor)}. Tried to add '{newTransactionType}' to '{oldTransactionType}'")
        {
        }
    }
}
