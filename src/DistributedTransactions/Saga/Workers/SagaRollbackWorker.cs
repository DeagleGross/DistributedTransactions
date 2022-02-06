using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Saga.Builders;
using DistributedTransactions.Saga.Executors;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Saga.Workers
{
    public sealed class SagaRollbackWorker
    {
        private readonly ILogger<SagaRollbackWorker> _logger;
        private readonly SagaExecutorBuilder _executorBuilder;

        public SagaRollbackWorker(SagaExecutorBuilder executorBuilder, ILogger<SagaRollbackWorker> logger)
        {
            _executorBuilder = executorBuilder;
            _logger = logger;
        }

        public async Task RollbackHistoryTransactions(CancellationToken cancellationToken)
        {
            var transactionProvider = _executorBuilder.TransactionProvider;

            // picking transactions, that you need to rollback
            var transactionsToRollbackEnumerable = await transactionProvider.GetAllByStatuses(
                new[] { TransactionStatus.NeedsToRollback, TransactionStatus.FailedToRollback },
                cancellationToken
            );
            var transactionsToRollback = transactionsToRollbackEnumerable.ToArray();

            if (transactionsToRollback.IsNullOrEmpty())
            {
                _logger.LogInformation($"No transactions with status '{TransactionStatus.NeedsToRollback}' found, so finishing work...");
                return;
            }

            foreach (var transactionToRollback in transactionsToRollback)
            {
                // clearing inter-transactional data, so that there is no data-colliding.
                // it is not expected to affect an execution, but user can use library wrong (i.e. use transactionContext in rollbackAsync),
                // so we need to do this just for safety
                _executorBuilder.TransactionContext.ClearInterTransactionalData();

                var transactionExecutor = new SagaExecutor(_executorBuilder.Logger,
                    _executorBuilder.Settings,
                    _executorBuilder.TransactionContext,
                    _executorBuilder.TransactionProvider,
                    _executorBuilder.OperationProvider);

                await transactionExecutor.RollbackTransactionAsync(transactionToRollback, cancellationToken);
            }
        }
    }
}
