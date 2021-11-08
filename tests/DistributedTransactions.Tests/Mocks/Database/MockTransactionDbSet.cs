using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public class MockTransactionDbSet : IList<TransactionEntity>
    {
        private readonly IList<TransactionEntity> _items = new List<TransactionEntity>();

        public void Add(TransactionEntity item)
        {
            item.Id = _items.Count;
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(TransactionEntity item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(TransactionEntity[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TransactionEntity item)
        {
            return _items.Remove(item);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public void GetById(long id) => _items.FirstOrDefault(x => x.Id == id);

        public void RemoveById(long id)
        {
            var itemToRemove = _items.FirstOrDefault(x => x.Id == id);
            if (itemToRemove is null) return;
            _items.Remove(itemToRemove);
        }

        public IEnumerator<TransactionEntity> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public int IndexOf(TransactionEntity item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, TransactionEntity item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public TransactionEntity this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
    }
}
