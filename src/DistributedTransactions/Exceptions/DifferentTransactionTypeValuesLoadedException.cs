using DistributedTransactions.Saga.Executors;
using System;

namespace DistributedTransactions.Exceptions
{
    public class DifferentTransactionTypeValuesLoadedException : Exception
    {
        public DifferentTransactionTypeValuesLoadedException(string oldTransactionType, string newTransactionType)
            : base($"Can not load operations with different transaction types in a single {nameof(SagaExecutor)}. Tried to add '{newTransactionType}' to '{oldTransactionType}'")
        {
        }
    }
}
