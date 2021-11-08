using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Models.Abstractions
{
    public interface IDistributedTransactionOperationExecutor 
    {
        Task CommitAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
