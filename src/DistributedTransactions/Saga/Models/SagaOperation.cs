using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Saga.Models.Abstractions;

namespace DistributedTransactions.Saga.Models
{
    internal class SagaOperation : ISagaOperation
    {
        public ISagaOperationExecutor OperationExecutor { get; set; }

        public object RollbackData { get; set; }

        public SagaOperation(ISagaOperationExecutor operationExecutor, object rollbackData)
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
