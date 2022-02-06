using System;
using DistributedTransactions.Providers;
using DistributedTransactions.Saga.Builders;

namespace DistributedTransactions.Exceptions
{
    public class NoOperationProviderRegisteredInBuilderException : Exception
    {
        public NoOperationProviderRegisteredInBuilderException()
            : base($"No {nameof(OperationProvider)} registered in {nameof(SagaExecutorBuilder)}")
        {
        }
    }
}
