using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Models
{
    internal class DistributedTransactionOperation : IDistributedTransactionOperation
    {
        public IDistributedTransactionOperationExecutor OperationExecutor { get; set; }

        public object RollbackData { get; set; }

        public DistributedTransactionOperation(IDistributedTransactionOperationExecutor operationExecutor, object rollbackData)
        {
            OperationExecutor = operationExecutor;
            RollbackData = rollbackData;
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await OperationExecutor.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await OperationExecutor.RollbackAsync(cancellationToken);
        }
    }
}
