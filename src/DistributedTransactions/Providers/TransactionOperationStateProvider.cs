using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Providers.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransactions.Providers
{
    internal class TransactionOperationStateProvider : ITransactionOperationStateProvider
    {
        private readonly IOperationRepository _transactionOperationRepository;

        public TransactionOperationStateProvider(IOperationRepository transactionOperationRepository)
        {
            _transactionOperationRepository = transactionOperationRepository;
        }

        public async Task<DistributedTransactionOperationState<T>> GetAsync<T>(DistributedTransactionOperationInfo operationInfo, CancellationToken cancellationToken) where T : class
        {
            throw new NotImplementedException();

            //var transactionOperationStateEntity = await _transactionOperationRepository.QueryAsync(new SelectTransactionOperationStateQuery
            //{
            //    OperationId = operationInfo.OperationId,
            //    TransactionGroupId = operationInfo.TransactionGroupId,
            //    OperationPriority = operationInfo.OperatorPriority
            //}, cancellationToken);
            //return DistributedTransactionOperationStateConverter.ToTransactionOperationStateModel<T>(transactionOperationStateEntity);
        }

        public async Task AddAsync<T>(DistributedTransactionOperationState<T> transactionOperationState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //var transactionOperationStateEntity = DistributedTransactionOperationStateConverter.ToTransactionOperationStateEntity(transactionOperationState);
            //await _transactionOperationRepository.InsertAsync(transactionOperationStateEntity, cancellationToken);
        }

        public async Task ChangeState<T>(DistributedTransactionOperationState<T> transactionOperationState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task ChangeState(long transactionTypeId, int operationOrder, OperationState newState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
