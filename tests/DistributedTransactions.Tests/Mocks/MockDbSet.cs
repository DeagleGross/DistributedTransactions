using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DistributedTransactions.Tests.Mocks
{
    public class MockDbSet<T> : IList<T>
    {
        private readonly List<T> _items = new();

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        public void Add(T item)
        {
            _items.Add(item);
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

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
        }

        public IEnumerable<T> Get(Func<T, bool> predicate)
        {
            return _items.Where(predicate);
        }

        public void Remove(Func<T, bool> predicate)
        {
            var suitableItems = _items.Where(predicate).ToArray();

            // leave it as for, because CollectionWasModifiedException will occure
            for (var i = 0; i < suitableItems.Length; i++)
            {
                var itemToRemove = suitableItems[i];
                _items.Remove(itemToRemove);
            }
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
