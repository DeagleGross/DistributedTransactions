using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTransactions.Exceptions
{
    public class RollbackDataPropertyNotFoundException : Exception
    {
        public RollbackDataPropertyNotFoundException(Type executorType, Type rollbackType)
            : base($"Could not find rollback property for executorType '{executorType}' with rollbackType '{rollbackType}'")
        {
        }
    }
}
