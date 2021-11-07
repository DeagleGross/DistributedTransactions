using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Providers.Abstractions;
using Microsoft.Extensions.Logging;

namespace DistributedTransactions.Executors
{
    public class DistributedTransactionExecutor
    {
        private readonly LinkedList<OperationExecutorWithInfo> _distributedTransactionOperationWrappers = new();

        internal ILogger<DistributedTransactionExecutor> Logger { get; set; }
        
        internal IOperationProvider OperationProvider { get; set; }
        
        internal ITransactionProvider TransactionProvider { get; set; }

        internal string TransactionType { get; set; }

        /// <summary>
        /// Registers operation in a transaction. Sensitive to order of registering - operations are executed in order of addition.
        /// </summary>
        /// <param name="operation">OperationExecutor with [DistributedTransactionExecutor] attribute to register in a transaction</param>
        public void RegisterOperation(IDistributedTransactionOperation operation)
        {
            var operationInfo = AttributeInfoRetriever.GetDistributedTransactionAttributeInfo(operation);

            if (TransactionType is null)
            {
                TransactionType = operationInfo.TransactionType;
            }
            else if (TransactionType != operationInfo.TransactionType)
            {
                throw new DifferentTransactionTypeValuesLoadedException(TransactionType, operationInfo.TransactionType);
            }

            var operationWrapper = new OperationExecutorWithInfo(operation, operationInfo);

            _distributedTransactionOperationWrappers.AddLast(operationWrapper);
        }

        public async Task ExecuteFullTransactionAsync(CancellationToken cancellationToken)
        {
            if (_distributedTransactionOperationWrappers.IsNullOrEmpty()) throw new NoTransactionOperationsRegisteredException();
            var currentNode = _distributedTransactionOperationWrappers.First;

            // if there are operations in transaction, it is valid
            // so firstly saving it as a new entity in database
            var transaction = await TransactionProvider.CreateAsync(new Transaction
            {
                TransactionType = TransactionType,
            }, cancellationToken);

            while (currentNode is not null)
            {
                var operationWrapper = currentNode.Value;

                if (await TryCommitOperationAsync(operationWrapper, cancellationToken))
                {
                    // commiting went succesfully, but there is possibility that we need to rollback it
                    // so we have to save data for further update
                    var operation = operationWrapper.OperationExecutor;
                    var operationinfo = operationWrapper.OperationInfo;


                }
                else
                {
                    // failed to commit operation, so we need to rollback all operations, that were commited before
                }

                currentNode = currentNode.Next;
            }
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
    }
}
