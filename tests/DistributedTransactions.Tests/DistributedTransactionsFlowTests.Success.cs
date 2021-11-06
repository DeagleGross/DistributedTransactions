using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
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
        private MockDatabase _mockDatabase;

        [SetUp]
        public void Setup()
        {
            _mockDatabase = new MockDatabase();
        }

        [Test]
        public async Task CreateManufacturer_CreateAutos_Success()
        {
            var distributedTransaction = DistributedTransaction.Create(GetLogger<DistributedTransaction>(),
                TransactionOperationStateProvider);

            var createManufacturer = new CreateManufacturer
            {
                MockDatabase = _mockDatabase,
                Manufacturer = new Manufacturer
                {
                    Id = 1,
                    Name = "Audi"
                }
            };

            var createAuto = new CreateAuto
            {
                MockDatabase = _mockDatabase,
                Auto = new Auto
                {
                    Id = 1,
                    ManufacturerId = 1,
                    Name = "A7"
                }
            };

            distributedTransaction.RegisterOperation(createManufacturer);
            distributedTransaction.RegisterOperation(createAuto);

            await distributedTransaction.ExecuteFullTransactionAsync(CancellationToken.None);

            // TODO check all data is loaded to mock_database correctly
        }

        [DistributedTransaction(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer), typeof(long))]
        internal class CreateManufacturer : IDistributedTransactionOperation
        {
            public Manufacturer Manufacturer { get; set; }

            public MockDatabase MockDatabase { get; set; }

            public long CreatedManufacturerId { get; set; }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Manufacturers.Add(Manufacturer);
                CreatedManufacturerId = Manufacturer.Id;
                return Task.CompletedTask;
            }

            public Task RollbackAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Manufacturers.RemoveById(CreatedManufacturerId);
                return Task.CompletedTask;
            }
        }

        [DistributedTransaction(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto), typeof(long))]
        internal class CreateAuto : IDistributedTransactionOperation
        {
            public Auto Auto { get; set; }

            public MockDatabase MockDatabase { get; set; }

            public long CreatedAutoId { get; set; }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Autos.Add(Auto);
                
                // saving as rollback data
                CreatedAutoId = Auto.Id;

                return Task.CompletedTask;
            }

            public Task RollbackAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Autos.RemoveById(CreatedAutoId);
                return Task.CompletedTask;
            }
        }

        public enum OperationType
        {
            CreateManufacturer,
            CreateAuto
        }

        public enum TransactionType
        {
            CreateManufacturerWithAuto
        }
    }
}