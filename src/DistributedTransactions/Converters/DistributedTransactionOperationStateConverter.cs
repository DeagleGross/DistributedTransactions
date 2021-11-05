using DistributedTransactions.DAL.Models;
using DistributedTransactions.Models;

namespace DistributedTransactions.Converters
{
    public static partial class DistributedTransactionOperationStateConverter
    {
        internal static DistributedTransactionOperationState<T> ToTransactionOperationStateModel<T>(OperationEntity entity)
            where T : class => new()
            {
                //OperationId = entity.OperationId,
                //TransactionGroupId = entity.TransactionGroupId,
                //OperationPriority = entity.RollbackOperationPriority,
                //RollbackDataType = Type.GetType(entity.RollbackDataType),
                //RollbackData = entity.RollbackData.Deserialize<T>(),
                //State = Enum.Parse<OperationState>(entity.State)
            };

        internal static OperationEntity ToTransactionOperationStateEntity<T>(DistributedTransactionOperationState<T> model) => new()
        {
            //OperationId = model.OperationId,
            //TransactionGroupId = model.TransactionGroupId,
            //RollbackOperationPriority = model.OperationPriority,
            //RollbackDataType = model.RollbackData.GetType().ToString(),
            //RollbackData = model.RollbackData.Serialize(),
            //State = model.State.ToString()
        };
    }
}
