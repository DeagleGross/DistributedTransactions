using System;

namespace DistributedTransactions.Exceptions
{
    public class RollbackInfraPropertyNotFoundException : Exception
    {
        public RollbackInfraPropertyNotFoundException(Type executorType, Type rollbackInfraType)
            : base($"Could not find rollback_infra property for executorType '{executorType}' with rollbackInfraType '{rollbackInfraType}'")
        {
        }
    }
}
