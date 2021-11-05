using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public static partial class MockDatabase
    {
        private static readonly IList<TransactionEntity> _transactionStatusEntities = new List<TransactionEntity>();

        public static void Add(TransactionEntity transactionEntity)
        {
            _transactionStatusEntities.Add(transactionEntity);
        }

        public static void Add(IEnumerable<TransactionEntity> transactionStatusEntities)
        {
            foreach (var transactionStatusEntity in transactionStatusEntities)
            {
                Add(transactionStatusEntity);
            }
        }

        public static void GetTransactionStatusEntityById(long id) => _transactionStatusEntities.FirstOrDefault(x => x.Id == id);
    }
}
