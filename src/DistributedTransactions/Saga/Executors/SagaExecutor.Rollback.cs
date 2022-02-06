using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;
using DistributedTransactions.Reflection;
using DistributedTransactions.Saga.Models.Abstractions;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Saga.Executors
{
    public partial class SagaExecutor
    {
        public async Task RollbackTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            _transaction = transaction;
            var transactionId = transaction.Id;

            var operationsToRollbackEnumerable = await _operationProvider.GetByTransactionIdAndStatusAsync(
                transactionId, new [] { OperationStatus.NeedsToRollback, OperationStatus.FailedToRollback }, cancellationToken);
            var operationsToRollback = operationsToRollbackEnumerable.ToArray();

            if (operationsToRollback.IsNullOrEmpty())
            {
                _logger.LogWarning("No operations to rollback found for transaction");
                return;
            }

            foreach (var operation in operationsToRollback)
            {
                // we need to create an instance of an IDistributedTransactionOperation, so that we can invoke Rollback method
                var operationExecutor = TypeHelper.GetOperationExecutorWithFilledData(operation, TransactionContext);

                if (await TryRollbackOperationAsync(operationExecutor, operation, cancellationToken))
                {
                    await _operationProvider.UpdateOperationStatus(operation, OperationStatus.Rollbacked, cancellationToken);
                    _logger.LogInformation($"Rollbacked \"{operation}\" successfully.");
                }
                else
                {
                    _logger.LogError($"Failed to rollback one of the operations during transaction(#id={transactionId}). Stopping the whole rollback process.");

                    // we have to set `failed to rollback` status to make sure we can determine such manual-to-rollback transactions
                    if (operation.Status is OperationStatus.NeedsToRollback)
                        await _operationProvider.UpdateOperationStatus(operation, OperationStatus.FailedToRollback, cancellationToken);

                    // and we have to set the same status for !transaction!
                    if (_transaction.Status is TransactionStatus.NeedsToRollback)
                        await _transactionProvider.UpdateTransactionStatus(transaction, TransactionStatus.FailedToRollback, cancellationToken);

                    return;
                }
            }

            _logger.LogInformation($"Successfully rollbacked all of the operations for transaction {transactionId}. Saving transaction as {TransactionStatus.FinishedWithRollback}");
            await _transactionProvider.UpdateTransactionStatus(_transaction, TransactionStatus.FinishedWithRollback, cancellationToken);
        }

        private async Task<bool> TryRollbackOperationAsync(ISagaOperationExecutor operationExecutor, Operation operation, CancellationToken cancellationToken)
        {
            // with retry flow
            if (TryGetRetryPolicy(out var retryPolicy, OperationActionType.Rollback))
            {
                return await TryExecuteWithHandlingExceptionAsync(async () =>
                {
                    await retryPolicy.ExecuteAsync(async () => { await operationExecutor.RollbackAsync(cancellationToken); });
                }, operation, OperationActionType.Rollback);
            }

            return await TryExecuteWithHandlingExceptionAsync(async () =>
            {
                await operationExecutor.RollbackAsync(cancellationToken);
            }, operation, OperationActionType.Rollback);
        }
    }
}
