using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Tests.Base;
using DistributedTransactions.Tests.Mocks.Database;
using DistributedTransactions.Tests.Mocks.Models;
using NUnit.Framework;

namespace DistributedTransactions.Tests
{
    [TestFixture]
    internal class DistributedTransactionsSuccessFlowTests : DistributedTransactionsTestsBase
    {
        [Test]
        public async Task CreateManufacturer_CreateAutos_Success()
        {
            var manufacturer = new Manufacturer
            {
                Id = 1,
                Name = "Audi"
            };

            var auto = new Auto
            {
                Id = 1,
                ManufacturerId = 1,
                Name = "A7"
            };

            var distributedTransaction = DistributedTransaction.Create(GetLogger<DistributedTransaction>(), TransactionOperationStateProvider);

            var createManufacturer = new CreateManufacturer();
            var createAuto = new CreateAuto();

            distributedTransaction.RegisterOperation(createManufacturer);
            distributedTransaction.RegisterOperation(createAuto);

            await distributedTransaction.ExecuteFullTransactionAsync(CancellationToken.None);
        }

        private class CreateManufacturer : IDistributedTransactionOperation
        {
            private Manufacturer Manufacturer { get; set; }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Add(Manufacturer);

                return Task.CompletedTask;
            }

            public Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public IDistributedTransactionOperation Create() => new CreateManufacturer();
        }

        private class CreateAuto : IDistributedTransactionOperation
        {
            public Task CommitAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public IDistributedTransactionOperation Create()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
