using System;
using DistributedTransactions.Providers;
using DistributedTransactions.Saga.Builders;

namespace DistributedTransactions.Exceptions
{
    public class NoTransactionProviderRegisteredInBuilderException : Exception
    {
        public NoTransactionProviderRegisteredInBuilderException()
            : base($"No {nameof(TransactionProvider)} registered in {nameof(SagaExecutorBuilder)}")
        {
        }
    }
}
