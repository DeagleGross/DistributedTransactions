using DistributedTransactions.Providers;
using DistributedTransactions.Providers.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedTransactions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDistributedTransactions(this IServiceCollection services)
        {
            services.AddScoped<IOperationProvider, OperationProvider>();
            services.AddScoped<ITransactionProvider, TransactionProvider>();

            services.AddScoped<ITransactionContext, ServiceTransactionContext>();
        }
    }
}
