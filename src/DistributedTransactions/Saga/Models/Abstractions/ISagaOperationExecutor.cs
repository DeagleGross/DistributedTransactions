using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Saga.Models.Abstractions
{
    public interface ISagaOperationExecutor
    {
        Task CommitAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
