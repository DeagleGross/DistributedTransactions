using System;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Extensions;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Builders;
using DistributedTransactions.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DistributedTransactions.Tests.Base
{
    [SetUpFixture]
    public abstract class DistributedTransactionsTestsBase
    {
        protected ServiceProvider ServiceProvider;

        protected IOperationProvider OperationProvider => ServiceProvider.GetRequiredService<IOperationProvider>();

        protected ITransactionProvider TransactionProvider => ServiceProvider.GetRequiredService<ITransactionProvider>();

        protected MockDatabase MockDatabase => ServiceProvider.GetRequiredService<MockDatabase>();

        protected ITransactionContext TransactionContext => ServiceProvider.GetRequiredService<ITransactionContext>();

        protected ILogger<T> GetLogger<T>() => ServiceProvider.GetRequiredService<ILogger<T>>();

        protected SagaExecutorBuilder SagaExecutorBuilder => ServiceProvider.GetRequiredService<SagaExecutorBuilder>();

        protected void OneTimeSetup(RetryPolicy retryPolicy = null)
        {
            ServiceProvider = SetupServiceProvider(retryPolicy);
        }

        private ServiceProvider SetupServiceProvider(RetryPolicy retryPolicy)
        {
            var services = new ServiceCollection();
            AddSerilogLogging(services);

            // testing services
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IOperationRepository, OperationRepository>();

            // this is where all of public types for DistributedTransactions library are loaded
            services.AddSaga(retryPolicy: retryPolicy);

            // mock database for mock-models just for testing on some sample examples
            // view Auto and Manufacturer classes in this project
            services.AddSingleton<MockDatabase>();

            return services.BuildServiceProvider();
        }

        private static void AddSerilogLogging(IServiceCollection services)
        {
            Serilog.ILogger logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Verbose()
                .CreateLogger();

            ILogger microsoftLogger = new SerilogLoggerFactory(logger).CreateLogger<DistributedTransactionsTestsBase>();
            services.AddLogging(builder => builder.ClearProviders().AddSerilog(logger).SetMinimumLevel(LogLevel.Trace));
            services.AddSingleton(microsoftLogger);
        }
    }
}
