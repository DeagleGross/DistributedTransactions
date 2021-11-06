using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Models.Abstractions
{
    public interface IDistributedTransactionOperation
    {
        Task CommitAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
