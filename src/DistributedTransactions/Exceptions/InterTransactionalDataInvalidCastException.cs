using System;
using DistributedTransactions.Providers.Abstractions;

namespace DistributedTransactions.Exceptions
{
    public class InterTransactionalDataInvalidCastException : Exception
    {
        public InterTransactionalDataInvalidCastException(InvalidCastException e, object dataObject)
            : base($"There was an invalid cast at '{nameof(ITransactionContext)}'. InterTransactional data type is {dataObject.GetType()}. Error: {e}")
        {
        }
    }
}
