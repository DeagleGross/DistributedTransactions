using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Providers.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Converters;

namespace DistributedTransactions.Providers
{
    internal class OperationProvider : IOperationProvider
    {
        private readonly IOperationRepository _operationRepository;

        public OperationProvider(IOperationRepository operationRepository)
        {
            _operationRepository = operationRepository;
        }

        public async Task<Operation<T>> GetByOperationIdAsync<T>(long operationId, CancellationToken cancellationToken) where T : class
        {
            var operationEntity = await _operationRepository.GetByOperationIdAsync(operationId, cancellationToken);
            return OperationConverter.FromEntity<T>(operationEntity);
        }

        public async Task<Operation<T>> CreateAsync<T>(Operation<T> operation, CancellationToken cancellationToken) where T : class
        {
            var operationEntity = await _operationRepository.CreateAsync(OperationConverter.ToEntity(operation), cancellationToken);
            return OperationConverter.FromEntity<T>(operationEntity);
        }
    }
}
