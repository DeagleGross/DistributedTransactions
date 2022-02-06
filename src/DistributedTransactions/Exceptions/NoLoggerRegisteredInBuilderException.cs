using System;
using DistributedTransactions.Saga.Builders;
using DistributedTransactions.Saga.Executors;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Exceptions
{
    public class NoLoggerRegisteredInBuilderException : Exception
    {
        public NoLoggerRegisteredInBuilderException()
            : base($"No {nameof(ILogger<SagaExecutor>)} registered in {nameof(SagaExecutorBuilder)}")
        {
        }
    }
}
