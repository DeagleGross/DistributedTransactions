using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Builders;
using DistributedTransactions.Executors;
using DistributedTransactions.Models;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Tests.Base;
using DistributedTransactions.Tests.Mocks;
using DistributedTransactions.Tests.Mocks.Database;
using DistributedTransactions.Tests.Mocks.Models;
using FluentAssertions;
using NUnit.Framework;

namespace DistributedTransactions.Tests
{
    [TestFixture]
    internal class DistributedTransactionsSuccessFlowTests : DistributedTransactionsTestsBase
    {
        private MockDatabase _mockDatabase;

        [SetUp]
        public new void Setup()
        {
            _mockDatabase = new MockDatabase();
        }

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

            var distributedTransactionExecutor = DistributedTransactionExecutorBuilder
                .CreateDistributedTransactionExecutor()
                .UseLogger(GetLogger<DistributedTransactionExecutor>())
                .UseTransactionProvider(TransactionProvider)
                .UseOperationProvider(OperationProvider);

            var createManufacturer = new CreateManufacturer
            {
                MockDatabase = _mockDatabase,
                Manufacturer = manufacturer
            };

            var createAuto = new CreateAuto
            {
                MockDatabase = _mockDatabase,
                Auto = auto
            };

            distributedTransactionExecutor.RegisterOperation(createManufacturer);
            distributedTransactionExecutor.RegisterOperation(createAuto);

            await distributedTransactionExecutor.ExecuteFullTransactionAsync(CancellationToken.None);

            var manufacturerInDb = _mockDatabase.Manufacturers.GetById(manufacturer.Id);
            manufacturerInDb.Should().Be(manufacturer);

            var autoInDb = _mockDatabase.Autos.GetById(auto.Id);
            autoInDb.Should().Be(auto);
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer), typeof(long))]
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

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto), typeof(long))]
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
    }
}