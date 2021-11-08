using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Providers.Abstractions;
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

        public async Task<Operation> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken)
        {
            var operationEntity = await _operationRepository.GetByOperationIdAsync(operationId, cancellationToken);
            return OperationConverter.FromEntity(operationEntity);
        }

        public async Task<IEnumerable<Operation>> GetByTransactionIdAndStatusAsync(long transactionId, OperationStatus operationStatus, CancellationToken cancellationToken)
        {
            var operationEntities = await _operationRepository.GetByTransactionIdAndStatus(transactionId, operationStatus.ToString(), cancellationToken);
            return operationEntities.Select(OperationConverter.FromEntity);
        }

        public async Task<Operation> CreateAsync(Operation operation, CancellationToken cancellationToken)
        {
            // if it is created, we can assume to save a new operation with a `created` status
            operation.Status = OperationStatus.Committed;

            var operationEntity = await _operationRepository.CreateAsync(OperationConverter.ToEntity(operation), cancellationToken);

            return OperationConverter.FromEntity(operationEntity);
        }

        public async Task UpdateOperationStatus(long operationId, OperationStatus status, CancellationToken cancellationToken)
        {
            await _operationRepository.UpdateOperationStatus(operationId, status.ToString(), cancellationToken);
        }

        public async Task UpdateOperationsStatus(IEnumerable<long> operationIds, OperationStatus status, CancellationToken cancellationToken)
        {
            await _operationRepository.UpdateOperationsStatus(operationIds, status.ToString(), cancellationToken);
        }
    }
}
