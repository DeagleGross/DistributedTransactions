using DistributedTransactions.Models;
using DistributedTransactions.Models.Settings;

namespace DistributedTransactions.Metrics.Abstractions
{
    public interface IDistributedTransactionsMetricsSender
    {
        public void RecordTransactionStatusChange(
            DistributedTransactionServiceOwnerInfo serviceOwnerInfo,
            Transaction transaction,
            TransactionStatus status);

        public void RecordOperationStatusChange(
            DistributedTransactionServiceOwnerInfo serviceOwnerInfo,
            Operation operation,
            OperationStatus status);
    }
}
