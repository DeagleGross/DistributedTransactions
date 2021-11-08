using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Models.Abstractions
{
    public abstract class DistributedTransactionOperationBase<T> : IDistributedTransactionOperation<T>
    {
        public T RollbackData { get; set; }

        public DistributedTransactionOperationBase(){}

        public abstract Task CommitAsync(CancellationToken cancellationToken);

        public abstract Task RollbackAsync(CancellationToken cancellationToken);
    }
}
