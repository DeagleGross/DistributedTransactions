using System;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Models;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Models.Abstractions;

namespace DistributedTransactions.Reflection
{
    internal static class TypeHelper
    {
        public static Type GetTypeFromDistributedTransactionOperation<TRollbackData>(ISagaOperation<TRollbackData> _)
        {
            return typeof(TRollbackData);
        }

        public static ISagaOperationExecutor GetOperationExecutorWithFilledData(Operation operation, ITransactionContext transactionContext)
        {
            var instance = Activator.CreateInstance(operation.ExecutorType, transactionContext);

            var rollbackDataProperty = operation.ExecutorType.GetProperty(nameof(ISagaOperation.RollbackData));
            if (rollbackDataProperty is null) throw new RollbackDataPropertyNotFoundException(operation.ExecutorType, operation.RollbackDataType);
            rollbackDataProperty.SetValue(instance, operation.RollbackData);

            return instance as ISagaOperationExecutor;
        }
    }
}
