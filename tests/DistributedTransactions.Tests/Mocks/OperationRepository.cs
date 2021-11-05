using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks
{
    public class OperationRepository : IOperationRepository
    {
        public Task<OperationEntity> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OperationEntity>> GetByTransactionId(long transactionId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(OperationEntity entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
