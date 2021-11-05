using DistributedTransactions.Attributes;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Providers.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Models
{
    public class DistributedTransaction
    {
        private readonly ILogger<DistributedTransaction> _logger;
        private readonly ITransactionOperationStateProvider _transactionOperationStateProvider;

        private readonly LinkedList<DistributedTranscationOperationWrapper> _distributedTransactionOperationWrappers;

        private DistributedTransaction(ILogger<DistributedTransaction> logger, ITransactionOperationStateProvider transactionOperationStateProvider)
        {
            _logger = logger;
            _transactionOperationStateProvider = transactionOperationStateProvider;
            _distributedTransactionOperationWrappers = new();
        }

        public static DistributedTransaction Create(ILogger<DistributedTransaction> logger, ITransactionOperationStateProvider provider) => new(logger, provider);

        /// <summary>
        /// Registers operation in a transaction. Sensitive to order of registering - operations are executed in order of addition.
        /// </summary>
        /// <param name="operation">Operation with [DistributedTransaction] attribute to register in a transaction</param>
        public void RegisterOperation(IDistributedTransactionOperation operation)
        {
            var operationInfo = AttributeInfoRetriever.GetDistributedTransactionAttributeInfo(operation);
            var operationWrapper = new DistributedTranscationOperationWrapper(operation, operationInfo);

            _distributedTransactionOperationWrappers.AddLast(operationWrapper);
        }

        public async Task ExecuteFullTransactionAsync(CancellationToken cancellationToken)
        {
            if (_distributedTransactionOperationWrappers.IsNullOrEmpty()) throw new NoTransactionOperationsRegisteredException();
            var currentNode = _distributedTransactionOperationWrappers.First;

            while (currentNode is not null)
            {
                var operationWrapper = currentNode.Value;

                if (await TryCommitOperationAsync(operationWrapper, cancellationToken))
                {
                    // commiting went succesfully, but there is possibility that we need to rollback it
                    // so we have to save data for further update
                    var operation = operationWrapper.Operation;
                    var operationinfo = operationWrapper.OperationInfo;


                }
                else
                {
                    // failed to commit operation, so we need to rollback all operations, that were commited before
                }

                currentNode = currentNode.Next;
            }
        }

        private async Task<bool> TryCommitOperationAsync(DistributedTranscationOperationWrapper operationWrapper, CancellationToken cancellationToken)
        {
            var operation = operationWrapper.Operation;
            var operationInfo = operationWrapper.OperationInfo;

            try
            {
                await operation.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                _logger.LogError($"Failed while '{nameof(operation.CommitAsync)}' execution of operation \"{operationInfo}\"");
                return false;
            }
        }
    }
}
