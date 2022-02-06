using System;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;
using DistributedTransactions.Saga.Models;
using DistributedTransactions.Saga.Models.Abstractions;

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
            RollbackPriority = operation.RollbackOperationPriority,
            ExecutionStage = operation.ExecutionStage,
            // rollback_data
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
                RollbackOperationPriority = entity.RollbackPriority,
                ExecutionStage = entity.ExecutionStage,
                ExecutorType = executorType,
                // rollback_data
                RollbackDataType = rollbackDataType,
                RollbackData = entity.RollbackData.Deserialize(rollbackDataType),
                Status = Enum.TryParse<OperationStatus>(entity.Status, true, out var status) ? status : throw new ArgumentException(entity.Status)
            };
        }

        public static ISagaOperation ToObjectOrientedOperation<TRollbackData>(ISagaOperation<TRollbackData> genericOperation)
        {
            return new SagaOperation(genericOperation, genericOperation.RollbackData);
        }
    }
}
