using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Extensions;
using DistributedTransactions.Providers.Abstractions;
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
    internal abstract class DistributedTransactionsTestsBase
    {
        private ServiceProvider _serviceProvider;

        protected IOperationProvider OperationProvider => _serviceProvider.GetRequiredService<IOperationProvider>();
        protected ITransactionProvider TransactionProvider => _serviceProvider.GetRequiredService<ITransactionProvider>();
        
        protected IOperationRepository OperationRepository => _serviceProvider.GetRequiredService<IOperationRepository>();
        protected ILogger<T> GetLogger<T>() => _serviceProvider.GetRequiredService<ILogger<T>>();

        [OneTimeSetUp]
        protected void Setup()
        {
            _serviceProvider = SetupServiceProvider();
        }

        private ServiceProvider SetupServiceProvider()
        {
            var services = new ServiceCollection();
            AddSerilogLogging(services);

            services.AddDistributedTransactions();
            services.AddScoped<IOperationRepository, OperationRepository>();

            return services.BuildServiceProvider();
        }

        private void AddSerilogLogging(IServiceCollection services)
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
