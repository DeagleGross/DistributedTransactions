using System;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Reflection
{
    internal static class TypeHelper
    {
        public static Type GetTypeFromDistributedTransactionOperation<T>(IDistributedTransactionOperation<T> operation)
        {
            return typeof(T);
        }

        public static IDistributedTransactionOperationExecutor GetOperationExecutor(Type executorType, Type rollbackType, object rollbackData)
        {
            var genericDistributedTransactionOperationType = typeof(IDistributedTransactionOperation<>);
            var instance = Activator.CreateInstance(executorType);

            var rollbackDataProperty = executorType.GetProperty(nameof(IDistributedTransactionOperation.RollbackData));
            if (rollbackDataProperty is null) throw new RollbackDataPropertyNotFoundException(executorType, rollbackType);
            rollbackDataProperty.SetValue(instance, rollbackData);

            return instance as IDistributedTransactionOperationExecutor;
        }

        private static Type[] GetGenericTypeArgumentsFromInstanceImplementedInterfaces(Type objectType, Type interfaceType)
        {
            var specificInterface = objectType.GetInterface(interfaceType.Name);
            return specificInterface?.GenericTypeArguments;
        }
    }
}
