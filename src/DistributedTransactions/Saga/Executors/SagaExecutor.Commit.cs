using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;
using DistributedTransactions.Saga.Models;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Saga.Executors
{
    public partial class SagaExecutor
    {
        /// <summary>
        /// Runs all registered operations.
        /// First of all staged operations are launched in parallel in ascending order, after that all non-staged operations are executed in order of their registration.
        /// When commit of any operation fails, the rollback process is immediatelly started for this specific transaction
        /// </summary>
        /// <param name="cancellationToken">token of cancellation for async-flows</param>
        /// <exception cref="NoTransactionOperationsRegisteredException">No operations were registered, so there is nothing to execute</exception>
        public async Task ExecuteTransactionAsync(CancellationToken cancellationToken)
        {
            if (_stagedOperationsMap.IsNullOrEmpty() && _nonStagedOperations.IsNullOrEmpty()) throw new NoTransactionOperationsRegisteredException();

            // if there are operations in transaction, it is valid
            // so firstly saving it as a new entity in database
            _transaction = await _transactionProvider.CreateAsync(new Transaction
            {
                TransactionType = _registeringTransactionType
            }, cancellationToken);

            var operationsCreated = new List<Operation>();
            await ExecuteStagedOperations(operationsCreated, cancellationToken);
            await ExecuteNonStagedOperations(operationsCreated, cancellationToken);

            // `Created` status means there were no errors during execution of operations
            // so setting `FinishedCorrectly` status
            if (_transaction.Status is TransactionStatus.Created)
            {
                await _transactionProvider.UpdateTransactionStatus(_transaction, TransactionStatus.FinishedCorrectly, cancellationToken);
                _logger.LogInformation($"Successfully finished transaction #{_transaction.Id}");
            }
        }

        private async Task ExecuteStagedOperations(ICollection<Operation> operationsCreated, CancellationToken cancellationToken)
        {
            if (_stagedOperationsMap.IsNullOrEmpty()) return;

            // by now skipping operations with not defined execution stage
            foreach (var (stageNumber, currentStageOperations) in _stagedOperationsMap)
            {
                _logger.LogInformation($"Starting to execute stage #{stageNumber} of transaction #{_transaction.Id}");

                var currentStageOperationTasks = currentStageOperations
                    .Select(operationWrapper => TryCommitAndSaveOperation(operationWrapper, operationsCreated, cancellationToken))
                    .ToList();

                var isCommittedOperationResults = await Task.WhenAll(currentStageOperationTasks);
                if (isCommittedOperationResults.Any(x => x is false))
                {
                    await HandleFailedCommitsAndRunRollbackAsync(operationsCreated, cancellationToken);
                }

                if (await HasPrematurelyFinishedAndUpdatedStatus(cancellationToken)) break;
            }
        }

        private async Task ExecuteNonStagedOperations(ICollection<Operation> operationsCreated, CancellationToken cancellationToken)
        {
            if (_nonStagedOperations.IsNullOrEmpty()) return;

            foreach (var nonStagedOperation in _nonStagedOperations)
            {
                if (!await TryCommitAndSaveOperation(nonStagedOperation, operationsCreated, cancellationToken))
                {
                    await HandleFailedCommitsAndRunRollbackAsync(operationsCreated, cancellationToken);
                }

                if (await HasPrematurelyFinishedAndUpdatedStatus(cancellationToken)) break;
            }
        }

        private async Task<bool> TryCommitAndSaveOperation(
            SagaOperationExecutorWithInfo operationWrapper,
            ICollection<Operation> operationsCreated,
            CancellationToken cancellationToken)
        {
            if (!await RunCommitOperationAsync(operationWrapper, cancellationToken)) return false;

            // committing was successful
            // but there is possibility that we need to rollback it
            // so we have to save data for further update
            var operation = operationWrapper.OperationExecutor;
            var operationInfo = operationWrapper.OperationInfo;

            var operationCreated = await _operationProvider.CreateAsync(new Operation
            {
                TransactionId = _transaction.Id,
                OperationType = operationInfo.OperationType,
                RollbackOperationPriority = operationInfo.RollbackOperationPriority,
                ExecutionStage = operationInfo.ExecutionStage,
                ExecutorType = operationInfo.ExecutorType,
                RollbackDataType = operationInfo.RollbackDataType,
                RollbackData = operation.RollbackData
            }, cancellationToken);

            operationsCreated.Add(operationCreated);
            _logger.LogInformation($"Successfully committed operation #{operationCreated.Id}");

            return true;
        }

        private async Task HandleFailedCommitsAndRunRollbackAsync(
            ICollection<Operation> operationsCreated,
            CancellationToken cancellationToken)
        {
            // failed to commit operation, so we need to rollback all operations, that were committed before
            _logger.LogError($"Failed to commit operation in a transaction #{_transaction.Id}. Starting rollback process.");

            // no operations were commited, so there is nothing to rollback
            // this means transaction has failed on the first operation
            // so we need to mark transaction as `Failed` and stop any actions for transaction
            if (operationsCreated.IsNullOrEmpty())
            {
                await _transactionProvider.UpdateTransactionStatus(_transaction, TransactionStatus.Failed, cancellationToken);
                return;
            }

            // firstly we need to mark transaction with new status - `NeedsToBeRollback-ed`
            // and also all of operations just in case we could not rollback and we could get back to that job later
            await _transactionProvider.UpdateTransactionStatus(_transaction, TransactionStatus.NeedsToRollback, cancellationToken);
            await _operationProvider.UpdateOperationsStatus(operationsCreated, OperationStatus.NeedsToRollback, cancellationToken);

            await RollbackTransactionAsync(_transaction, cancellationToken);
        }

        private async Task<bool> RunCommitOperationAsync(SagaOperationExecutorWithInfo sagaOperationExecutorWithInfo, CancellationToken cancellationToken)
        {
            var operation = sagaOperationExecutorWithInfo.OperationExecutor;
            var operationInfo = sagaOperationExecutorWithInfo.OperationInfo;

            // with retry flow
            if (TryGetRetryPolicy(out var retryPolicy, OperationActionType.Commit))
            {
                return await TryExecuteWithHandlingExceptionAsync(async () =>
                {
                    await retryPolicy.ExecuteAsync(async () => { await operation.CommitAsync(cancellationToken); });
                }, operationInfo, OperationActionType.Commit);
            }

            // single try action execution
            return await TryExecuteWithHandlingExceptionAsync(async () =>
            {
                await operation.CommitAsync(cancellationToken);
            }, operationInfo, OperationActionType.Commit);
        }
    }
}
