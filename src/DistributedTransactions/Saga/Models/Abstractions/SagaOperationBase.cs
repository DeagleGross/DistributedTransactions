using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Providers.Abstractions;

namespace DistributedTransactions.Saga.Models.Abstractions
{
    public abstract class SagaOperationBase<TRollbackData> : ISagaOperation<TRollbackData>, INotifyPropertyChanged
    {
        private TRollbackData _rollbackData;

        public TRollbackData RollbackData
        {
            get => _rollbackData;
            set => SetField(ref _rollbackData, value);
        }

        protected SagaOperationBase(ITransactionContext transactionContext)
        {
        }

        public abstract Task CommitAsync(CancellationToken cancellationToken);

        public abstract Task RollbackAsync(CancellationToken cancellationToken);

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetField<TField>(ref TField field, TField value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<TField>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
