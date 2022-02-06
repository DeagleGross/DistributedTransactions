using DistributedTransactions.DAL.Models;
using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks
{
    public class MockDatabase
    {
        public MockDbSet<Auto> Autos { get; } = new();

        public MockDbSet<Manufacturer> Manufacturers { get; } = new();

        public MockDbSet<OperationEntity> Operations { get; } = new();

        public MockDbSet<TransactionEntity> Transactions { get; } = new();

        public void Clear()
        {
            Autos.Clear();
            Manufacturers.Clear();

            Operations.Clear();
            Transactions.Clear();
        }
    }
}
