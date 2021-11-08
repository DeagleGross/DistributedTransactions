using System.Collections.Generic;
using DistributedTransactions.DAL.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.DAL.Abstractions
{
    public interface IOperationRepository
    {
        Task<OperationEntity> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken);

        Task<IEnumerable<OperationEntity>> GetByTransactionIdAndStatus(long transactionId, string operationStatus, CancellationToken cancellationToken);

        Task<OperationEntity> CreateAsync(OperationEntity entity, CancellationToken cancellationToken);

        Task UpdateOperationStatus(long operationId, string status, CancellationToken cancellationToken);

        Task UpdateOperationsStatus(IEnumerable<long> operationIds, string status, CancellationToken cancellationToken);
    }
}
