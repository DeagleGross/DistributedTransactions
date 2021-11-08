using System;
using System.Linq;
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
    internal class DistributedTransactionsRollbackFlowTests : DistributedTransactionsTestsBase
    {
        private MockDatabase _mockDatabase;

        [SetUp]
        public void Setup()
        {
            _mockDatabase = MockDatabase;
            _mockDatabase.ClearAll();
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

            _mockDatabase.Transactions.Should().NotBeEmpty();
            _mockDatabase.Transactions.First().Should().NotBeNull();
            _mockDatabase.Transactions.First().Status.Should().Be(TransactionStatus.FinishedWithRollback.ToString());
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
        internal class CreateManufacturer : IDistributedTransactionOperation<long>
        {
            public Manufacturer Manufacturer { get; set; }

            public MockDatabase MockDatabase { get; set; }

            public long RollbackData { get; set; }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Manufacturers.Add(Manufacturer);
                
                // saving as rollback data
                RollbackData = Manufacturer.Id;

                return Task.CompletedTask;
            }

            public Task RollbackAsync(CancellationToken cancellationToken)
            {
                MockDatabase.Manufacturers.RemoveById(RollbackData);
                return Task.CompletedTask;
            }
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto))]
        internal class CreateAuto : IDistributedTransactionOperation<long>
        {
            public Auto Auto { get; set; }

            public MockDatabase MockDatabase { get; set; }

            public long RollbackData { get; set; }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}