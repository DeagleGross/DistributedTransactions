using System;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Abstractions;

namespace DistributedTransactions.Converters
{
    internal static class OperationConverter
    {
        public static OperationEntity ToEntity(Operation operation) => new()
        {
            Id = operation.Id,
            TransactionId = operation.TransactionId,
            OperationType = operation.OperationType,
            ExecutorType = operation.ExecutorType.AssemblyQualifiedName,
            RollbackOperationPriority = operation.RollbackOperationPriority,
            RollbackDataType = operation.RollbackDataType.AssemblyQualifiedName,
            RollbackData = operation.RollbackData.Serialize(operation.RollbackDataType),
            Status = operation.Status.ToString()
        };

        public static Operation FromEntity(OperationEntity entity)
        {
            var rollbackDataType = Type.GetType(entity.RollbackDataType);
            var executorType = Type.GetType(entity.ExecutorType);

            return new()
            {
                Id = entity.Id,
                TransactionId = entity.TransactionId,
                OperationType = entity.OperationType,
                RollbackOperationPriority = entity.RollbackOperationPriority,
                ExecutorType = executorType,
                RollbackDataType = rollbackDataType,
                RollbackData = entity.RollbackData.Deserialize(rollbackDataType),
                Status = Enum.Parse<OperationStatus>(entity.Status)
            };
        }

        public static IDistributedTransactionOperation ToObjectInstanceOperation<T>(IDistributedTransactionOperation<T> genericOperation)
        {
            return new DistributedTransactionOperation(genericOperation, genericOperation.RollbackData);
        }
    }
}
