using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Converters;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Metrics.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers.Abstractions;

namespace DistributedTransactions.Providers
{
    internal class OperationProvider : IOperationProvider
    {
        private readonly IOperationRepository _operationRepository;

        private DistributedTransactionServiceOwnerInfo _distributedTransactionServiceOwnerInfo;
        private IDistributedTransactionsMetricsSender _distributedTransactionsMetricsSender;

        public OperationProvider(IOperationRepository operationRepository)
        {
            _operationRepository = operationRepository;
        }

        DistributedTransactionServiceOwnerInfo IOperationProvider.DistributedTransactionServiceOwnerInfo
        {
            get => _distributedTransactionServiceOwnerInfo;
            set => _distributedTransactionServiceOwnerInfo = value;
        }

        IDistributedTransactionsMetricsSender IOperationProvider.DistributedTransactionsMetricsSender
        {
            get => _distributedTransactionsMetricsSender;
            set => _distributedTransactionsMetricsSender = value;
        }

        public async Task<IEnumerable<Operation>> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken)
        {
            var operationEntities = await _operationRepository.GetByTransactionId(transactionId, cancellationToken);
            return operationEntities.Select(OperationConverter.FromEntity);
        }

        public async Task<IEnumerable<Operation>> GetByTransactionIdAndStatusAsync(long transactionId, OperationStatus[] operationStatuses, CancellationToken cancellationToken)
        {
            var operationEntities = await _operationRepository.GetByTransactionIdAndStatus(
                transactionId,
                operationStatuses.Select(x => x.ToString()).ToArray(),
                cancellationToken);

            return operationEntities.Select(OperationConverter.FromEntity);
        }

        public async Task<Operation> CreateAsync(Operation operation, CancellationToken cancellationToken)
        {
            // if it is created, we can assume to save a new operation with a `commited` status
            operation.Status = OperationStatus.Committed;

            var operationEntity = await _operationRepository.CreateAsync(OperationConverter.ToEntity(operation), cancellationToken);
            SendStatusChangeTrackingMetric(operation, OperationStatus.Committed);

            return OperationConverter.FromEntity(operationEntity);
        }

        public async Task UpdateOperationStatus(Operation operation, OperationStatus status, CancellationToken cancellationToken)
        {
            await _operationRepository.UpdateOperationStatus(operation.Id, status.ToString(), cancellationToken);
            SendStatusChangeTrackingMetric(operation, status);
        }

        public async Task UpdateOperationsStatus(ICollection<Operation> operations, OperationStatus status, CancellationToken cancellationToken)
        {
            await _operationRepository.UpdateOperationsStatuses(operations.Select(x => x.Id), status.ToString(), cancellationToken);

            foreach (var operation in operations)
                SendStatusChangeTrackingMetric(operation, status);
        }

        private void SendStatusChangeTrackingMetric(Operation operation, OperationStatus status)
        {
            if (_distributedTransactionsMetricsSender is null || _distributedTransactionServiceOwnerInfo is null) return;
            _distributedTransactionsMetricsSender.RecordOperationStatusChange(_distributedTransactionServiceOwnerInfo, operation, status);
        }
    }
}
