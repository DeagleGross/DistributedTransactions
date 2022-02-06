using System;

namespace DistributedTransactions.Exceptions
{
    public class InterTransactionalDataKeyAlreadyExistsException : Exception
    {
        public InterTransactionalDataKeyAlreadyExistsException(string key)
            : base($"Key '{key}' already exists in inter-transactional data dictionary")
        {
        }
    }
}
