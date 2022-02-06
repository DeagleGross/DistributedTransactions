using System;

namespace DistributedTransactions.Exceptions
{
    public class AttributeNotFound<TAttribute> : Exception
        where TAttribute : Attribute
    {
        public AttributeNotFound(Type instanceType)
            : base($"No '{nameof(TAttribute)}' found for '{instanceType}'")
        {
        }
    }
}
