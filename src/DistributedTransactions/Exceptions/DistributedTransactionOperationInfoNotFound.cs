using DistributedTransactions.Attributes;
using System;

namespace DistributedTransactions.Exceptions
{
    public class DistributedTransactionOperationInfoNotFound : Exception
    {
        public DistributedTransactionOperationInfoNotFound(Type operationType) 
            : base($"No '{nameof(DistributedTransactionOperationAttribute)}' found for operation {operationType}")
        {
        }
    }
}
