using System;

namespace DistributedTransactions.Exceptions
{
    public class NoTransactionOperationsRegisteredException : Exception
    {
        public NoTransactionOperationsRegisteredException()
            : base($"No operations were registered in a transaction. Nothing to execute")
        {
        }
    }
}
