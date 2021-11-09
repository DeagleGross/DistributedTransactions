using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public class MockDbSet<T> where T : IMockModel
    {
        private readonly IList<T> _items = new List<T>();

        public void Add(T item)
        {
            item.Id = _items.Count + 1;
            _items.Add(item);
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _items.Add(item);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        public T GetById(long id) => _items.FirstOrDefault(x => x.Id == id);

        public void RemoveById(long id)
        {
            var itemToRemove = _items.FirstOrDefault(x => x.Id == id);
            if (itemToRemove is null) return;
            _items.Remove(itemToRemove);
        }
    }
}
