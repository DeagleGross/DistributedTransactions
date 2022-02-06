using DistributedTransactions.Exceptions;
using DistributedTransactions.Metrics.Abstractions;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Executors;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Saga.Builders
{
    public class SagaExecutorBuilder
    {
        internal ITransactionContext TransactionContext { get; private set; }

        internal ILogger<SagaExecutor> Logger { get; private set; }

        internal ITransactionProvider TransactionProvider { get; private set; }

        internal IOperationProvider OperationProvider { get; private set; }

        internal DistributedTransactionSettings Settings { get; }

        internal IDistributedTransactionsMetricsSender DistributedTransactionsMetricsSender { get; private set; }

        /// <summary>
        /// The only possible way to create a builder
        /// </summary>
        public SagaExecutorBuilder(DistributedTransactionSettings settings)
        {
            Settings = settings;
        }

        public SagaExecutorBuilder AddRetryPolicy(RetryPolicy retryPolicy)
        {
            Settings.RetryPolicy = retryPolicy;
            return this;
        }

        public SagaExecutorBuilder AddSagaServiceOwner(DistributedTransactionServiceOwnerInfo distributedTransactionServiceOwnerInfo)
        {
            Settings.ServiceOwnerInfo = distributedTransactionServiceOwnerInfo;
            return this;
        }

        public SagaExecutorBuilder AddMetricsSender(IDistributedTransactionsMetricsSender metricsSender)
        {
            DistributedTransactionsMetricsSender = metricsSender;

            if (TransactionProvider is not null) TransactionProvider.DistributedTransactionsMetricsSender = metricsSender;
            if (OperationProvider is not null) OperationProvider.DistributedTransactionsMetricsSender = metricsSender;

            return this;
        }

        public SagaExecutorBuilder AddTransactionContext(ITransactionContext transactionContext)
        {
            TransactionContext = transactionContext;
            return this;
        }

        public SagaExecutorBuilder AddLogger(ILogger<SagaExecutor> executorLogger)
        {
            Logger = executorLogger;
            return this;
        }

        public SagaExecutorBuilder AddTransactionProvider(ITransactionProvider transactionProvider)
        {
            TransactionProvider = transactionProvider;

            if (DistributedTransactionsMetricsSender is not null) TransactionProvider.DistributedTransactionsMetricsSender = DistributedTransactionsMetricsSender;
            if (Settings?.ServiceOwnerInfo is not null) TransactionProvider.DistributedTransactionServiceOwnerInfo = Settings.ServiceOwnerInfo;

            return this;
        }

        public SagaExecutorBuilder AddOperationProvider(IOperationProvider operationProvider)
        {
            OperationProvider = operationProvider;

            if (DistributedTransactionsMetricsSender is not null) OperationProvider.DistributedTransactionsMetricsSender = DistributedTransactionsMetricsSender;
            if (Settings?.ServiceOwnerInfo is not null) OperationProvider.DistributedTransactionServiceOwnerInfo = Settings.ServiceOwnerInfo;

            return this;
        }

        /// <summary>
        /// Validates all of required properties for <see cref="SagaExecutor"/> are added in builder
        /// and returns a transaction instance, that you can use further
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NoLoggerRegisteredInBuilderException"> <see cref="ILogger{DistributedTransactionExecutor}"/> wasn't added to transactionExecutorBuilder </exception>
        /// <exception cref="NoOperationProviderRegisteredInBuilderException"> <see cref="IOperationProvider"/> wasn't added to transactionExecutorBuilder </exception>
        /// <exception cref="NoTransactionProviderRegisteredInBuilderException"> <see cref="ITransactionProvider"/> wasn't added to transactionExecutorBuilder </exception>
        public SagaExecutor ValidateAndBuild()
        {
            if (Logger is null) throw new NoLoggerRegisteredInBuilderException();
            if (OperationProvider is null) throw new NoOperationProviderRegisteredInBuilderException();
            if (TransactionContext is null) throw new NoTransactionProviderRegisteredInBuilderException();

            return new SagaExecutor(Logger, Settings, TransactionContext, TransactionProvider, OperationProvider);
        }
    }
}
