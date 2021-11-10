using System;
using DistributedTransactions.Providers.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedTransactions.Providers
{
    /// <summary>
    /// Dependency injection container for services used in ASP.NET applications.
    /// Delegates an implementation to <see cref="IServiceProvider"/>.
    /// </summary>
    internal class ServiceTransactionContext : ITransactionContext
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceTransactionContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetRequiredService<T>() => _serviceProvider.GetRequiredService<T>();
    }
}
