using System;

namespace DistributedTransactions.Exceptions
{
    public class InterTransactionalDataKeyNotFoundException : Exception
    {
        public InterTransactionalDataKeyNotFoundException(string key)
            : base($"Key '{key}' not found in inter-transactional data")
        {
        }
    }
}
