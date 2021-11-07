using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.DAL.Abstractions
{
    public interface ITransactionRepository
    {
        Task<TransactionEntity> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken);

        Task<IEnumerable<TransactionEntity>> GetByTransactionGroupId(long transactionGroupId, CancellationToken cancellationToken);

        Task<TransactionEntity> CreateAsync(TransactionEntity entity, CancellationToken cancellationToken);
    }
}
