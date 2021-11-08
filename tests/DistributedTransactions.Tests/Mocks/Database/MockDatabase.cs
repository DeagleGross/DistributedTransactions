using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public class MockDatabase
    {
        public MockDbSet<Auto> Autos { get; set; }

        public MockDbSet<Manufacturer> Manufacturers { get; set; }

        public MockTransactionDbSet Transactions { get; set; }

        public MockOperationDbSet Operations { get; set; }

        public void ClearAll()
        {
            Autos.Clear();
            Manufacturers.Clear();
            Transactions.Clear();
            Operations.Clear();
        }

        public MockDatabase()
        {
            Autos = new MockDbSet<Auto>();
            Manufacturers = new MockDbSet<Manufacturer>();
            Transactions = new MockTransactionDbSet();
            Operations = new MockOperationDbSet();
        }
    }
}