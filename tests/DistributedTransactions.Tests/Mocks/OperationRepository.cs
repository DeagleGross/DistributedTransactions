using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Tests.Mocks.Database;

namespace DistributedTransactions.Tests.Mocks
{
    public class OperationRepository : IOperationRepository
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
    }
}
