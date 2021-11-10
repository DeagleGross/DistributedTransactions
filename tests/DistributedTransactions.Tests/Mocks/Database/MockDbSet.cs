using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public class MockDbSet<T> : IList<T> where T : IMockModel
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

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _items.Remove(item);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public T GetById(long id) => _items.FirstOrDefault(x => x.Id == id);

        public void RemoveById(long id)
        {
            var itemToRemove = _items.FirstOrDefault(x => x.Id == id);
            if (itemToRemove is null) return;
            _items.Remove(itemToRemove);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
    }
}
