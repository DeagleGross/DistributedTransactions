using System;

namespace DistributedTransactions.Exceptions
{
    public class RollbackDataPropertyNotFoundException : Exception
    {
        public RollbackDataPropertyNotFoundException(Type executorType, Type rollbackType)
            : base($"Could not find rollback_data property for executorType '{executorType}' with rollbackType '{rollbackType}'")
        {
        }
    }
}
