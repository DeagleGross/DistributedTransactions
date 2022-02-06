using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Tests.Mocks
{
    internal class OperationRepository : IOperationRepository
    {
        private readonly MockDatabase _mockDatabase;

        public OperationRepository(MockDatabase mockDatabase)
        {
            _mockDatabase = mockDatabase;
        }

        public Task<OperationEntity> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockDatabase.Operations.FirstOrDefault(x => x.Id == operationId));
        }

        public Task<IEnumerable<OperationEntity>> GetByTransactionIdAndStatus(long transactionId, string operationStatus, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockDatabase.Operations
                .Where(x => x.TransactionId == transactionId)
                .Where(x => x.Status == operationStatus)
            );
        }

        public Task<IEnumerable<OperationEntity>> GetByTransactionId(long transactionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockDatabase.Operations.Where(x => x.TransactionId == transactionId));
        }

        public Task<OperationEntity> CreateAsync(OperationEntity entity, CancellationToken cancellationToken)
        {
            _mockDatabase.Operations.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateOperationStatus(long operationId, string status, CancellationToken cancellationToken)
        {
            _mockDatabase.Operations.First(x => x.Id == operationId).Status = status;
            return Task.CompletedTask;
        }

        public Task UpdateOperationsStatus(IEnumerable<long> operationIds, string status, CancellationToken cancellationToken)
        {
            foreach (var operation in _mockDatabase.Operations.Where(x => operationIds.Contains(x.Id)))
            {
                operation.Status = status;
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<OperationEntity>> GetByTransactionIdAndStatus(long transactionId, string[] operationStatuses, CancellationToken cancellationToken)
        {
            var operations = _mockDatabase.Operations
                .Where(x => x.TransactionId == transactionId)
                .Where(x => operationStatuses.Contains(x.Status));

            return Task.FromResult(operations);
        }

        public Task UpdateOperationsStatuses(IEnumerable<long> operationIds, string status, CancellationToken cancellationToken)
        {
            foreach (var operationId in operationIds)
            {
                var operation = _mockDatabase.Operations.FirstOrDefault(x => x.Id == operationId);
                if (operation is null) continue;
                operation.Status = status;
            }

            return Task.CompletedTask;
        }
    }
}
