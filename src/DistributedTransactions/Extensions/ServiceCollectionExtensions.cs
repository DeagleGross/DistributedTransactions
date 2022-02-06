using DistributedTransactions.Metrics.Abstractions;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Builders;
using DistributedTransactions.Saga.Executors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Extensions
{
    public static class ServiceCollectionExtensions
    {


        public static void AddSaga(this IServiceCollection services,
                                        DistributedTransactionServiceOwnerInfo distributedTransactionServiceOwnerInfo = null,
                                        RetryPolicy retryPolicy = null)
        {
            // context used for injecting user's services into user-defined operation classes
            services.TryAddScoped<ITransactionContext, ServiceTransactionContext>();

            // intermideate layer between logic models and repositories
            services.TryAddScoped<IOperationProvider, OperationProvider>();
            services.TryAddScoped<ITransactionProvider, TransactionProvider>();

            // add settings instance for service settings within distributed transactions library
            // and singleton metrics sender for prometheus recording stats
            services.TryAddSingleton(serviceProvider =>
            {
                return new DistributedTransactionSettings(distributedTransactionServiceOwnerInfo, retryPolicy);
            });

            // add builder through DI for easy usage with standard settings
            services.TryAddScoped(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<SagaExecutor>>();
                var distributedTransactionSettings = serviceProvider.GetService<DistributedTransactionSettings>();

                var transactionContext = serviceProvider.GetService<ITransactionContext>();
                var transactionProvider = serviceProvider.GetService<ITransactionProvider>();
                var operationProvider = serviceProvider.GetService<IOperationProvider>();
                var metricsSender = serviceProvider.GetService<IDistributedTransactionsMetricsSender>();

                return new SagaExecutorBuilder(distributedTransactionSettings)
                    .AddLogger(logger)
                    .AddTransactionContext(transactionContext)
                    .AddOperationProvider(operationProvider)
                    .AddTransactionProvider(transactionProvider)
                    .AddMetricsSender(metricsSender);
            });
        }
    }
}
