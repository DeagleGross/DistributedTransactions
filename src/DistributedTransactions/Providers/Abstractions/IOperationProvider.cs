using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Metrics.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Settings;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface IOperationProvider
    {
        internal DistributedTransactionServiceOwnerInfo DistributedTransactionServiceOwnerInfo { get; set; }

        internal IDistributedTransactionsMetricsSender DistributedTransactionsMetricsSender { get; set; }

        Task<IEnumerable<Operation>> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken);

        Task<IEnumerable<Operation>> GetByTransactionIdAndStatusAsync(long transactionId, OperationStatus[] statuses, CancellationToken cancellationToken);

        Task<Operation> CreateAsync(Operation operation, CancellationToken cancellationToken);

        Task UpdateOperationStatus(Operation operation, OperationStatus status, CancellationToken cancellationToken);

        Task UpdateOperationsStatus(ICollection<Operation> operations, OperationStatus status, CancellationToken cancellationToken);
    }
}
