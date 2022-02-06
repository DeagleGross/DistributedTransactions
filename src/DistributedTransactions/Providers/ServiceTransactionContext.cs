using System;
using System.Collections.Generic;
using DistributedTransactions.Exceptions;
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
        private readonly IDictionary<string, object> _interTransactionalData;
        private bool _isFinishedPrematurely;

        bool ITransactionContext.IsFinishedPrematurely => _isFinishedPrematurely;

        public ServiceTransactionContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _interTransactionalData = new Dictionary<string, object>();
        }

        public T GetRequiredService<T>() => _serviceProvider.GetRequiredService<T>();

        public void SaveInterTransactionalData(string key, object data)
        {
            if (_interTransactionalData.ContainsKey(key)) throw new InterTransactionalDataKeyAlreadyExistsException(key);
            _interTransactionalData[key] = data;
        }

        public T GetInterTransactionalData<T>(string key)
        {
            if (!_interTransactionalData.ContainsKey(key)) throw new InterTransactionalDataKeyNotFoundException(key);

            try
            {
                return (T)_interTransactionalData[key];
            }
            catch (InvalidCastException e)
            {
                throw new InterTransactionalDataInvalidCastException(e, _interTransactionalData[key]);
            }
        }

        public void FinishPrematurely()
        {
            _isFinishedPrematurely = true;
        }

        public void ClearInterTransactionalData()
        {
            _interTransactionalData.Clear();
        }
    }
}
