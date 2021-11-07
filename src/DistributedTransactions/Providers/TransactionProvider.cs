using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Converters;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Models;
using DistributedTransactions.Providers.Abstractions;

namespace DistributedTransactions.Providers
{
    internal class TransactionProvider : ITransactionProvider
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionProvider(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<Transaction> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken)
        {
            var transactionEntity = await _transactionRepository.GetByTransactionIdAsync(transactionId, cancellationToken);
            return TransactionConverter.FromEntity(transactionEntity);
        }

        public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            // if it is created, we can assume to save a new transaction with a `created` status
            transaction.Status = TransactionStatus.Created;

            var transactionEntity = await _transactionRepository.CreateAsync(TransactionConverter.ToEntity(transaction), cancellationToken);

            return TransactionConverter.FromEntity(transactionEntity);
        }
    }
}
