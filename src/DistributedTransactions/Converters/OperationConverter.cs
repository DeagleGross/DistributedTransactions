using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Helpers.Extensions;
using DistributedTransactions.Models;

namespace DistributedTransactions.Converters
{
    internal static class OperationConverter
    {
        public static OperationEntity ToEntity<T>(Operation<T> operation) => new()
        {
            Id = operation.Id,
            TransactionId = operation.TransactionId,
            OperationType = operation.OperationType,
            RollbackOperationPriority = operation.RollbackOperationPriority,
            RollbackDataType = operation.RollbackDataType.AssemblyQualifiedName,
            RollbackData = operation.RollbackData.Serialize(),
            Status = operation.Status.ToString()
        };

        public static Operation<T> FromEntity<T>(OperationEntity entity) where T : class
        {
            return new()
            {
                Id = entity.Id,
                TransactionId = entity.TransactionId,
                OperationType = entity.OperationType,
                RollbackOperationPriority = entity.RollbackOperationPriority,
                RollbackDataType = Type.GetType(entity.RollbackDataType),
                RollbackData = entity.RollbackData.Deserialize<T>(),
                Status = Enum.Parse<OperationStatus>(entity.Status)
            };
        }
    }
}
