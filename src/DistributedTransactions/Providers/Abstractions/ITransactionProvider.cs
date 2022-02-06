using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Metrics.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Settings;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface ITransactionProvider
    {
        internal DistributedTransactionServiceOwnerInfo DistributedTransactionServiceOwnerInfo { get; set; }

        internal IDistributedTransactionsMetricsSender DistributedTransactionsMetricsSender { get; set; }

        Task<Transaction> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken);

        Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken);

        Task UpdateTransactionStatus(Transaction transaction, TransactionStatus newStatus, CancellationToken cancellationToken);

        Task<IEnumerable<Transaction>> GetAllByStatuses(TransactionStatus[] statuses, CancellationToken cancellationToken);
    }
}
