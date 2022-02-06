using System;
using System.Collections.Generic;
using DistributedTransactions.Converters;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Reflection;
using DistributedTransactions.Saga.Models;
using DistributedTransactions.Saga.Models.Abstractions;

namespace DistributedTransactions.Saga.Executors
{
    public partial class SagaExecutor
    {
        /// <summary>
        /// Registers operation in a transaction. Sensitive to order of registering - operations are executed in order of addition.
        /// </summary>
        /// <param name="operation">OperationExecutor with [DistributedTransactionExecutor] attribute to register in a transaction</param>
        public void RegisterOperation<TRollbackData>(SagaOperationBase<TRollbackData> operation)
        {
            var operationInfo = InstanceInfoRetriever.GetOperationInfo(operation);

            // maintaining single transaction_type not to mess up during the transaction_execution process
            if (_registeringTransactionType is null)
            {
                _registeringTransactionType = operationInfo.TransactionType;
            }
            else if (_registeringTransactionType != operationInfo.TransactionType)
            {
                throw new DifferentTransactionTypeValuesLoadedException(_registeringTransactionType, operationInfo.TransactionType);
            }

            var operationWrapper = new SagaOperationExecutorWithInfo(OperationConverter.ToObjectOrientedOperation(operation), operationInfo);

            // handy approach to bind rollbackData of user-defined operation.rollbackData of `T` type to internal operation.rollbackData of `object` type.
            operation.PropertyChanged += (sender, args) =>
            {
                if (sender is null) throw new ArgumentNullException($"operation object passed when registering operation is null...");

                const string rollbackDataPropertyName = nameof(SagaOperationBase<object>.RollbackData);
                if (args.PropertyName == rollbackDataPropertyName)
                {
                    var genericOperationRollbackData = sender.GetType().GetProperty(rollbackDataPropertyName)!.GetValue(sender, null);
                    operationWrapper.OperationExecutor.RollbackData = genericOperationRollbackData;
                }
            };

            AddOperationToCorrespondingStorage(operationWrapper);
        }

        private void AddOperationToCorrespondingStorage(SagaOperationExecutorWithInfo operationWrapper)
        {
            var executionStage = operationWrapper.OperationInfo.ExecutionStage;
            if (executionStage.HasValue)
            {
                _stagedOperationsMap ??= new();

                // creating specific stage list of operations storage
                if (!_stagedOperationsMap.ContainsKey(executionStage.Value))
                    _stagedOperationsMap.Add(executionStage.Value, new List<SagaOperationExecutorWithInfo>());

                _stagedOperationsMap[executionStage.Value].Add(operationWrapper);
            }
            else
            {
                _nonStagedOperations ??= new();
                _nonStagedOperations.Add(operationWrapper);
            }
        }
    }
}
