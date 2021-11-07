using DistributedTransactions.Executors;
using DistributedTransactions.Providers.Abstractions;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Builders
{
    public static class DistributedTransactionExecutorBuilder
    {
        public static DistributedTransactionExecutor CreateDistributedTransactionExecutor()
        {
            return new DistributedTransactionExecutor();
        }

        public static DistributedTransactionExecutor UseLogger(this DistributedTransactionExecutor transactionExecutor, ILogger<DistributedTransactionExecutor> executorLogger)
        {
            transactionExecutor.Logger = executorLogger;
            return transactionExecutor;
        }

        public static DistributedTransactionExecutor UseTransactionProvider(this DistributedTransactionExecutor transactionExecutor, ITransactionProvider transactionProvider)
        {
            transactionExecutor.TransactionProvider = transactionProvider;
            return transactionExecutor;
        }

        public static DistributedTransactionExecutor UseOperationProvider(this DistributedTransactionExecutor transactionExecutor, IOperationProvider operationProvider)
        {
            transactionExecutor.OperationProvider= operationProvider;
            return transactionExecutor;
        }

        public static DistributedTransactionExecutor SetTransactionType(this DistributedTransactionExecutor transactionExecutor, string transactionType)
        {
            transactionExecutor.TransactionType = transactionType;
            return transactionExecutor;
        }
    }
}
