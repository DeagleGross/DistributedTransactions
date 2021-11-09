using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Annotations;

namespace DistributedTransactions.Models.Abstractions
{
    public abstract class DistributedTransactionOperationBase<T> : IDistributedTransactionOperation<T>, INotifyPropertyChanged
    {
        private T _rollbackData;

        public T RollbackData
        {
            get => _rollbackData;
            set => SetField(ref _rollbackData, value);
        }

        public DistributedTransactionOperationBase(){}

        public abstract Task CommitAsync(CancellationToken cancellationToken);

        public abstract Task RollbackAsync(CancellationToken cancellationToken);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<TField>(ref TField field, TField value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<TField>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
