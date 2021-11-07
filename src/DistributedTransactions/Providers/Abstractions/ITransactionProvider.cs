using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Models;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface ITransactionProvider
    {
        Task<Transaction> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken);

        Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken);
    }
}
