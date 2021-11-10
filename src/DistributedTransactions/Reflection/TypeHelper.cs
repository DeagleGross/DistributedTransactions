using System;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Providers.Abstractions;

namespace DistributedTransactions.Reflection
{
    internal static class TypeHelper
    {
        public static Type GetTypeFromDistributedTransactionOperation<TRollbackData>(IDistributedTransactionOperation<TRollbackData> _)
        {
            return typeof(TRollbackData);
        }

        public static IDistributedTransactionOperationExecutor GetOperationExecutorWithFilledData(Operation operation, ITransactionContext transactionContext)
        {
            var instance = Activator.CreateInstance(operation.ExecutorType, transactionContext);

            var rollbackDataProperty = operation.ExecutorType.GetProperty(nameof(IDistributedTransactionOperation.RollbackData));
            if (rollbackDataProperty is null) throw new RollbackDataPropertyNotFoundException(operation.ExecutorType, operation.RollbackDataType);
            rollbackDataProperty.SetValue(instance, operation.RollbackData);

            return instance as IDistributedTransactionOperationExecutor;
        }
    }
}
