using DistributedTransactions.Exceptions;
using DistributedTransactions.Models;
using System;

namespace DistributedTransactions.Attributes
{
    internal static class AttributeInfoRetriever
    {
        public static DistributedTransactionOperationInfo GetDistributedTransactionAttributeInfo(object instance)
        {
            var attribute = GetAttribute<DistributedTransactionAttribute>(instance);
            return new DistributedTransactionOperationInfo(attribute);
        }

        private static T GetAttribute<T>(object instance) where T : class
        {
            var instanceType = instance.GetType();
            var attribute = Attribute.GetCustomAttribute(instanceType, typeof(T)) as T;

            if (attribute is null) throw DistributedTransactionOperationInfoNotFound.Create(instanceType);

            return attribute;
        }
    }
}
