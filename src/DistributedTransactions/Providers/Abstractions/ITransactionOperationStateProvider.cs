using DistributedTransactions.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Providers.Abstractions
{
    public interface ITransactionOperationStateProvider
    {
        Task<DistributedTransactionOperationState<T>> GetAsync<T>(DistributedTransactionOperationInfo operationInfo, CancellationToken cancellationToken) where T : class;

        Task AddAsync<T>(DistributedTransactionOperationState<T> transactionOperationState, CancellationToken cancellationToken);
    }
}
