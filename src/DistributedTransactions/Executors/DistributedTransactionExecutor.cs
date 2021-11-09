using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Converters;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Reflection;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Executors
{
    public class DistributedTransactionExecutor
    {
        private readonly LinkedList<OperationExecutorWithInfo> _distributedTransactionOperationWrappers = new();

        private Transaction _transaction;
        private readonly IList<Operation> _operations = new List<Operation>();

        internal ILogger<DistributedTransactionExecutor> Logger { get; set; }

        internal IOperationProvider OperationProvider { get; set; }

        internal ITransactionProvider TransactionProvider { get; set; }

        internal string TransactionType { get; set; }

        /// <summary>
        /// Registers operation in a transaction. Sensitive to order of registering - operations are executed in order of addition.
        /// </summary>
        /// <param name="operation">OperationExecutor with [DistributedTransactionExecutor] attribute to register in a transaction</param>
        public void RegisterOperation<T>(DistributedTransactionOperationBase<T> operation)
        {
            var operationInfo = InstanceInfoRetriever.GetOperationInfo(operation);

            if (TransactionType is null)
            {
                TransactionType = operationInfo.TransactionType;
            }
            else if (TransactionType != operationInfo.TransactionType)
            {
                throw new DifferentTransactionTypeValuesLoadedException(TransactionType, operationInfo.TransactionType);
            }

            var operationWrapper = new OperationExecutorWithInfo(OperationConverter.ToObjectInstanceOperation(operation), operationInfo);

            operation.PropertyChanged += (sender, _) =>
            {
                if (sender is null) throw new ArgumentNullException($"operation object passed when registering operation is null...");

                var distributedOperationBaseRollbackDataPropertyName = nameof(DistributedTransactionOperationBase<object>.RollbackData);
                var genericOperationRollbackData = sender.GetType().GetProperty(distributedOperationBaseRollbackDataPropertyName)!.GetValue(sender, null);
                operationWrapper.OperationExecutor.RollbackData = genericOperationRollbackData;
            };

            _distributedTransactionOperationWrappers.AddLast(operationWrapper);
        }

        public async Task ExecuteFullTransactionAsync(CancellationToken cancellationToken)
        {
            if (_distributedTransactionOperationWrappers.IsNullOrEmpty()) throw new NoTransactionOperationsRegisteredException();
            var currentNode = _distributedTransactionOperationWrappers.First;

            // if there are operations in transaction, it is valid
            // so firstly saving it as a new entity in database
            _transaction = await TransactionProvider.CreateAsync(new Transaction
            {
                TransactionType = TransactionType
            }, cancellationToken);

            while (currentNode is not null)
            {
                var operationWrapper = currentNode.Value;

                if (await TryCommitOperationAsync(operationWrapper, cancellationToken))
                {
                    // commiting went successfully, but there is possibility that we need to rollback it
                    // so we have to save data for further update
                    var operation = operationWrapper.OperationExecutor;
                    var operationInfo = operationWrapper.OperationInfo;

                    var operationCreated = await OperationProvider.CreateAsync(new Operation
                    {
                        TransactionId = _transaction.Id,
                        OperationType = operationInfo.OperationType,
                        RollbackOperationPriority = operationInfo.RollbackOperationPriority,
                        ExecutorType = operationInfo.ExecutorType,
                        RollbackDataType = operationInfo.RollbackDataType,
                        RollbackData = operation.RollbackData
                    }, cancellationToken);

                    _operations.Add(operationCreated);
                    Logger.LogInformation($"Successfully committed operation #{operationCreated.Id}");
                }
                else
                {
                    // failed to commit operation, so we need to rollback all operations, that were committed before
                    Logger.LogError(
                        $"Failed to commit operation in a transaction #{_transaction.Id}. Starting rollback process.");

                    // firstly we need to mark transaction with new status - `NeedsToBeRollback-ed`
                    // and also all of operations just in case we could not rollback and we could get back to that job later
                    await TransactionProvider.UpdateTransactionStatus(_transaction.Id, TransactionStatus.NeedsToRollback, cancellationToken);
                    await OperationProvider.UpdateOperationsStatus(_operations.Select(x => x.Id), OperationStatus.NeedsToRollback, cancellationToken);

                    await RollbackTransaction(_transaction.Id, cancellationToken);
                    return;
                }

                currentNode = currentNode.Next;
            }

            // if finished correctly, then we need to change transaction status to `finished correctly`.
            // there is nothing more we need to do
            await TransactionProvider.UpdateTransactionStatus(_transaction.Id, TransactionStatus.FinishedCorrectly,
                cancellationToken);
            Logger.LogInformation($"Successfully finished transaction #{_transaction.Id}");
        }

        public async Task RollbackTransaction(long transactionId, CancellationToken cancellationToken)
        {
            var operationsToRollback = await OperationProvider.GetByTransactionIdAndStatusAsync(transactionId, OperationStatus.NeedsToRollback, cancellationToken);

            // ReSharper disable once PossibleMultipleEnumeration
            if (operationsToRollback.IsNullOrEmpty())
            {
                Logger.LogWarning($"No operations to rollback found for transaction");
                return;
            }

            foreach (var operation in operationsToRollback)
            {
                // we need to create an instance of an IDistributedTransactionOperation, so that we can invoke Rollback method
                var operationExecutor = TypeHelper.GetOperationExecutor(operation.ExecutorType, operation.RollbackDataType, operation.RollbackData);

                if (await TryRollbackOperationAsync(operationExecutor, operation, cancellationToken))
                {
                    await OperationProvider.UpdateOperationStatus(operation.Id, OperationStatus.Rollbacked, cancellationToken);
                    Logger.LogInformation($"Rollbacked \"{operation}\" successfully.");
                }
                else
                {
                    Logger.LogError($"Failed to rollback one of the operations during transaction(#id={transactionId}). Stopping the whole rollback process.");
                    return;
                }
            }

            Logger.LogInformation($"Successfully rollbacked all of the operations for transaction {transactionId}. Saving transaction as {TransactionStatus.FinishedWithRollback}");
            await TransactionProvider.UpdateTransactionStatus(transactionId, TransactionStatus.FinishedWithRollback, cancellationToken);
        }

        private async Task<bool> TryCommitOperationAsync(OperationExecutorWithInfo operationExecutorWithInfo, CancellationToken cancellationToken)
        {
            var operation = operationExecutorWithInfo.OperationExecutor;
            var operationInfo = operationExecutorWithInfo.OperationInfo;

            try
            {
                await operation.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                Logger.LogError($"Failed while '{nameof(operation.CommitAsync)}' execution of operation \"{operationInfo}\"");
                return false;
            }
        }

        private async Task<bool> TryRollbackOperationAsync(IDistributedTransactionOperationExecutor operationExecutor, Operation operation, CancellationToken cancellationToken)
        {
            try
            {
                await operationExecutor.RollbackAsync(cancellationToken);
                return true;
            }
            catch
            {
                Logger.LogError($"Failed while '{nameof(operationExecutor.RollbackAsync)}' execution of operation \"{operation}\"");
                return false;
            }
        }
    }
}
