using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    internal class MockTransactionDbSet
    {
        private readonly IList<TransactionEntity> _items = new List<TransactionEntity>();

        public void Add(TransactionEntity item)
        {
            _items.Add(item);
        }

        public void Add(IEnumerable<TransactionEntity> items)
        {
            foreach (var item in items)
            {
                _items.Add(item);
            }
        }

        public void GetById(long id) => _items.FirstOrDefault(x => x.Id == id);

        public void RemoveById(long id)
        {
            var itemToRemove = _items.FirstOrDefault(x => x.Id == id);
            if (itemToRemove is null) return;
            _items.Remove(itemToRemove);
        }
    }
}
