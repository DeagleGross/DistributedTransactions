using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    internal class MockOperationDbSet
    {
        private readonly IList<OperationEntity> _items = new List<OperationEntity>();

        public void Add(OperationEntity item)
        {
            _items.Add(item);
        }

        public void Add(IEnumerable<OperationEntity> items)
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
