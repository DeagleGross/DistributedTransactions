using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    internal class MockDatabase
    {
        public MockDbSet<Auto> Autos { get; set; }

        public MockDbSet<Manufacturer> Manufacturers { get; set; }

        public MockTransactionDbSet Transactions { get; set; }

        public MockOperationDbSet Operations { get; set; }

        public MockDatabase()
        {
            Autos = new MockDbSet<Auto>();
            Manufacturers = new MockDbSet<Manufacturer>();
            Transactions = new MockTransactionDbSet();
            Operations = new MockOperationDbSet();
        }
    }
}