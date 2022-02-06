using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace DistributedTransactions.Saga.Executors
{
    public partial class SagaExecutor
    {
        private readonly ILogger<SagaExecutor> _logger;
        private readonly DistributedTransactionSettings _settings;

        private readonly IOperationProvider _operationProvider;
        private readonly ITransactionProvider _transactionProvider;

        private List<SagaOperationExecutorWithInfo> _nonStagedOperations;
        private SortedDictionary<int, List<SagaOperationExecutorWithInfo>> _stagedOperationsMap;

        private Transaction _transaction;
        private string _registeringTransactionType;

        /// <summary>
        /// Storage for any data context, that could be passed between any operations in a transaction.
        /// Also this context is used for resolving types registered by dependency injection mechanism through `IServiceProvider`
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public ITransactionContext TransactionContext { get; }

        /// <summary>
        /// Last occured exception while performing actions of saga
        /// </summary>
        public Exception LastOccuredException { get; private set; }

        /// <summary>
        /// Identifier of transaction.
        /// It could be null if transcation was not created or registered properly.
        /// </summary>
        public long? TransactionId => _transaction?.Id;

        /// <summary>
        /// Status of transaction on which actions were performed by SagaExecutor.
        /// It could be null if transaction was not created or registered properly.
        /// </summary>
        public TransactionStatus? Status => _transaction?.Status;

        internal SagaExecutor(
            ILogger<SagaExecutor> logger,
            DistributedTransactionSettings settings,
            ITransactionContext transactionContext,
            ITransactionProvider transactionProvider,
            IOperationProvider operationProvider)
        {
            _logger = logger;
            _settings = settings;

            TransactionContext = transactionContext;
            _operationProvider = operationProvider;
            _transactionProvider = transactionProvider;
        }

        /// <summary>
        /// Setups polly retry using passed settings
        /// </summary>
        /// <param name="retryPolicy">polly policy that will be initialized</param>
        /// <param name="operationActionType">type of operation to select proper policy settings</param>
        /// <returns></returns>
        private bool TryGetRetryPolicy(out AsyncRetryPolicy retryPolicy, OperationActionType operationActionType)
        {
            if (!_settings.RetryPolicy.IsTurnedOn)
            {
                retryPolicy = null;
                return false;
            }

            var operationRetryCount = _settings.RetryPolicy.GetRetryCount(operationActionType);

            retryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync(operationRetryCount, (exception, retryIndex) =>
                {
                    _logger.LogError(exception, $"Failed while '{operationActionType}' execution of operation in a retry #{retryIndex}...");
                    LastOccuredException = exception;
                });

            return true;
        }

        private async Task<bool> TryExecuteWithHandlingExceptionAsync(Func<Task> executeAction, OperationInfo operationInfo, OperationActionType? actionType = null)
            => await TryExecuteWithHandlingExceptionAsync(executeAction, operationInfo.ToString(), actionType);

        private async Task<bool> TryExecuteWithHandlingExceptionAsync(Func<Task> executeAction, Operation operation, OperationActionType? actionType = null)
            => await TryExecuteWithHandlingExceptionAsync(executeAction, operation.ToString(), actionType);

        private async Task<bool> TryExecuteWithHandlingExceptionAsync(Func<Task> executeAction, string operationDescription, OperationActionType? actionType = null)
        {
            try
            {
                await executeAction();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while {actionType} operation execution \"{operationDescription}\"");
                LastOccuredException = e;
                return false;
            }
        }

        private async Task<bool> HasPrematurelyFinishedAndUpdatedStatus(CancellationToken cancellationToken)
        {
            if (!TransactionContext.IsFinishedPrematurely) return false;

            _logger.LogInformation("User has explicitly called to finish this transaction prematurely. Stopping any further execution process.");
            await _transactionProvider.UpdateTransactionStatus(_transaction, TransactionStatus.FinishedPrematurely, cancellationToken);
            return true;
        }
    }
}
