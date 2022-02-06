using DistributedTransactions.Attributes;
using DistributedTransactions.Models;
using DistributedTransactions.Saga.Models.Abstractions;

namespace DistributedTransactions.Reflection
{
    internal static class InstanceInfoRetriever
    {
        public static OperationInfo GetOperationInfo<TRollbackData>(ISagaOperation<TRollbackData> operation)
        {
            var attribute = AttributeInfoRetriever.GetAttribute<DistributedTransactionOperationAttribute>(operation);
            var rollbackDataType = TypeHelper.GetTypeFromDistributedTransactionOperation(operation);
            var executorType = operation.GetType();

            return new OperationInfo(attribute, rollbackDataType, executorType);
        }
    }
}
