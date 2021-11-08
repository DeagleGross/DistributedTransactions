using System.Collections.Generic;
using DistributedTransactions.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface IOperationProvider
    {
        Task<Operation> GetByOperationIdAsync(long operationId, CancellationToken cancellationToken);

        Task<IEnumerable<Operation>> GetByTransactionIdAndStatusAsync(long transactionId, OperationStatus status, CancellationToken cancellationToken);

        Task<Operation> CreateAsync(Operation operation, CancellationToken cancellationToken);

        Task UpdateOperationStatus(long operationId, OperationStatus status, CancellationToken cancellationToken);

        Task UpdateOperationsStatus(IEnumerable<long> operationIds, OperationStatus status, CancellationToken cancellationToken);
    }
}
