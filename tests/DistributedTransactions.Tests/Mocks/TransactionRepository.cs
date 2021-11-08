using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Tests.Mocks.Database;

namespace DistributedTransactions.Tests.Mocks
{
    public class TransactionRepository : ITransactionRepository
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
    }
}
