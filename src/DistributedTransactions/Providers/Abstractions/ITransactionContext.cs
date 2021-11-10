using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface ITransactionContext
    {
        T GetRequiredService<T>();
    }
}
