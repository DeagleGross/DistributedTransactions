using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.DAL.Abstractions
{
    public interface IOperationRepository
    {
        Task<OperationEntity> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken);

        Task<IEnumerable<OperationEntity>> GetByTransactionId(long transactionId, CancellationToken cancellationToken);

        Task<IEnumerable<OperationEntity>> GetByTransactionIdAndStatus(long transactionId, string[] operationStatuses, CancellationToken cancellationToken);

        Task<OperationEntity> CreateAsync(OperationEntity entity, CancellationToken cancellationToken);

        Task UpdateOperationStatus(long operationId, string status, CancellationToken cancellationToken);

        Task UpdateOperationsStatuses(IEnumerable<long> operationIds, string status, CancellationToken cancellationToken);
    }
}
