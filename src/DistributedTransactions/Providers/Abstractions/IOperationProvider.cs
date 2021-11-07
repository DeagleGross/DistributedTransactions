using DistributedTransactions.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface IOperationProvider
    {
        Task<Operation<T>> GetByOperationIdAsync<T>(long operationId, CancellationToken cancellationToken) 
            where T : class;

        Task<Operation<T>> CreateAsync<T>(Operation<T> operation, CancellationToken cancellationToken)
            where T : class;
    }
}
