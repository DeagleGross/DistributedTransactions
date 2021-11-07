using DistributedTransactions.Attributes;
using System;

namespace DistributedTransactions.Exceptions
{
    public class DistributedTransactionOperationInfoNotFound : Exception
    {
        private DistributedTransactionOperationInfoNotFound(string message) : base(message)
        {
        }

        public static DistributedTransactionOperationInfoNotFound Create(Type operationType) => new($"No '{nameof(DistributedTransactionOperationAttribute)}' found for operation {operationType}");

        public static DistributedTransactionOperationInfoNotFound Create(string operationType) => new($"No '{nameof(DistributedTransactionOperationAttribute)}' found for operation {operationType}");
    }
}
