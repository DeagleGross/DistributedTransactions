using System;
using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Reflection
{
    internal static class TypeHelper
    {
        public static Type GetTypeFromDistributedTransactionOperation<T>(IDistributedTransactionOperation<T> operation)
        {
            return typeof(T);
        }

        public static IDistributedTransactionOperationExecutor GetOperationExecutor(Type executorType, Type rollbackType)
        {
            var genericDistributedTransactionOperationType = typeof(IDistributedTransactionOperation<>);
            var instance = Activator.CreateInstance(executorType);

            // TODO don't forget to load rollback data to an instance here
            return instance as IDistributedTransactionOperationExecutor;
        }

        private static Type[] GetGenericTypeArgumentsFromInstanceImplementedInterfaces(Type objectType, Type interfaceType)
        {
            var specificInterface = objectType.GetInterface(interfaceType.Name);
            return specificInterface?.GenericTypeArguments;
        }
    }
}
