using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Converters;
using DistributedTransactions.DAL.Abstractions;
using DistributedTransactions.Metrics.Abstractions;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers.Abstractions;

namespace DistributedTransactions.Providers
{
    internal class TransactionProvider : ITransactionProvider
    {
        private readonly ITransactionRepository _transactionRepository;
        private DistributedTransactionServiceOwnerInfo _distributedTransactionServiceOwnerInfo;
        private IDistributedTransactionsMetricsSender _distributedTransactionsMetricsSender;

        public TransactionProvider(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        DistributedTransactionServiceOwnerInfo ITransactionProvider.DistributedTransactionServiceOwnerInfo
        {
            get => _distributedTransactionServiceOwnerInfo;
            set => _distributedTransactionServiceOwnerInfo = value;
        }

        IDistributedTransactionsMetricsSender ITransactionProvider.DistributedTransactionsMetricsSender
        {
            get => _distributedTransactionsMetricsSender;
            set => _distributedTransactionsMetricsSender = value;
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
            SendStatusChangeTrackingMetric(transaction, TransactionStatus.Created);

            return TransactionConverter.FromEntity(transactionEntity);
        }

        public async Task UpdateTransactionStatus(Transaction transaction, TransactionStatus newStatus, CancellationToken cancellationToken)
        {
            transaction.Status = newStatus;

            await _transactionRepository.UpdateTransactionStatusAsync(transaction.Id, newStatus.ToString(), cancellationToken);
            SendStatusChangeTrackingMetric(transaction, newStatus);
        }

        public async Task<IEnumerable<Transaction>> GetAllByStatuses(TransactionStatus[] statuses, CancellationToken cancellationToken)
        {
            var transactionEntities = await _transactionRepository.GetAllByStatuses(
                statuses.Select(x => x.ToString()).ToArray(),
                cancellationToken);

            return transactionEntities.Select(TransactionConverter.FromEntity);
        }

        private void SendStatusChangeTrackingMetric(Transaction transaction, TransactionStatus status)
        {
            if (_distributedTransactionsMetricsSender is null || _distributedTransactionServiceOwnerInfo is null) return;
            _distributedTransactionsMetricsSender.RecordTransactionStatusChange(_distributedTransactionServiceOwnerInfo, transaction, status);
        }
    }
}
