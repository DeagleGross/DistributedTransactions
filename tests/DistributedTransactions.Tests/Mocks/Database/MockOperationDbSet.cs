using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public class MockOperationDbSet : IList<OperationEntity>
    {
        private readonly IList<OperationEntity> _items = new List<OperationEntity>();

        public void GetById(long id) => _items.FirstOrDefault(x => x.Id == id);

        public void RemoveById(long id)
        {
            var itemToRemove = _items.FirstOrDefault(x => x.Id == id);
            if (itemToRemove is null) return;
            _items.Remove(itemToRemove);
        }

        public IEnumerator<OperationEntity> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public void Add(OperationEntity item)
        {
            item.Id = _items.Count + 1;
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(OperationEntity item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(OperationEntity[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(OperationEntity item)
        {
            return _items.Remove(item);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public int IndexOf(OperationEntity item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, OperationEntity item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public OperationEntity this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
    }
}
