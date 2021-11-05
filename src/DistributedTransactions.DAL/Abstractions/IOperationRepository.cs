using System.Collections.Generic;
using DistributedTransactions.DAL.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.DAL.Abstractions
{
    public interface IOperationRepository
    {
        Task<OperationEntity> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken);

        Task<IEnumerable<OperationEntity>> GetByTransactionId(long transactionId, CancellationToken cancellationToken);

        Task AddAsync(OperationEntity entity, CancellationToken cancellationToken);
    }
}
