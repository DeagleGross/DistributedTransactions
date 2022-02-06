using DistributedTransactions.Exceptions;
using System;

namespace DistributedTransactions.Attributes
{
    internal static class AttributeInfoRetriever
    {
        public static TAttribute GetAttribute<TAttribute>(object instance)
            where TAttribute : Attribute
        {
            var instanceType = instance.GetType();
            var attribute = Attribute.GetCustomAttribute(instanceType, typeof(TAttribute)) as TAttribute;
            if (attribute is null) throw new AttributeNotFound<TAttribute>(instanceType);

            return attribute;
        }
    }
}
