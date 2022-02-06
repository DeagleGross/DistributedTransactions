using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks
{
    internal class TransactionRepository : ITransactionRepository
    {
        private readonly MockDatabase _mockDatabase;

        public TransactionRepository(MockDatabase mockDatabase)
        {
            _mockDatabase = mockDatabase;
        }

        public Task<TransactionEntity> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockDatabase.Transactions.FirstOrDefault(x => x.Id == transactionId));
        }

        public Task<IEnumerable<TransactionEntity>> GetByTransactionGroupId(string transactionType, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockDatabase.Transactions.Where(x => x.TransactionType == transactionType));
        }

        public Task<TransactionEntity> CreateAsync(TransactionEntity entity, CancellationToken cancellationToken)
        {
            _mockDatabase.Transactions.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateTransactionStatusAsync(long transactionId, string newTransactionStatus, CancellationToken cancellationToken)
        {
            _mockDatabase.Transactions.FirstOrDefault(x => x.Id == transactionId)!.Status = newTransactionStatus;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TransactionEntity>> GetByTransactionType(string transactionType, CancellationToken cancellationToken)
        {
            var transactions = _mockDatabase.Transactions
                .Where(x => x.TransactionType == transactionType);

            return Task.FromResult(transactions);
        }

        public Task<IEnumerable<TransactionEntity>> GetAllByStatuses(string[] statuses, CancellationToken cancellationToken)
        {
            var transactions = _mockDatabase.Transactions
                .Where(x => statuses.Contains(x.Status));

            return Task.FromResult(transactions);
        }
    }
}
