using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Models.Abstractions;
using DistributedTransactions.Tests.Base;
using DistributedTransactions.Tests.Mocks;
using DistributedTransactions.Tests.Mocks.Models;
using FluentAssertions;
using NUnit.Framework;

namespace DistributedTransactions.Tests.Saga.Premature
{
    [TestFixture]
    internal class SagaPrematureFlowTests : DistributedTransactionsTestsBase
    {
        private ITransactionProvider _transactionProvider;
        private IOperationProvider _operationProvider;
        private MockDatabase _mockDatabase;

        [OneTimeSetUp]
        public void SetupServiceProvider()
        {
            OneTimeSetup();
        }

        [SetUp]
        public void Setup()
        {
            _transactionProvider = TransactionProvider;
            _operationProvider = OperationProvider;
            _mockDatabase = MockDatabase;

            _mockDatabase.Clear();
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

            var sagaExecutor = SagaExecutorBuilder.ValidateAndBuild();

            var createManufacturer = new CreateManufacturer(TransactionContext)
            {
                Manufacturer = manufacturer
            };

            var createAuto = new CreateAuto(TransactionContext)
            {
                Auto = auto
            };

            sagaExecutor.RegisterOperation(createManufacturer);
            sagaExecutor.RegisterOperation(createAuto);

            await sagaExecutor.ExecuteTransactionAsync(CancellationToken.None);

            // instance status
            sagaExecutor.Status.Should().Be(TransactionStatus.FinishedPrematurely);
            sagaExecutor.LastOccuredException.Should().BeNull();

            // in database entities
            var transactionId = sagaExecutor.TransactionId!.Value;
            var transaction = await _transactionProvider.GetByTransactionIdAsync(transactionId, CancellationToken.None);
            transaction.Status.Should().Be(TransactionStatus.FinishedPrematurely);

            var operationEntitiesEnumerable = await _operationProvider.GetByTransactionIdAsync(transactionId, CancellationToken.None);
            var operationEntities = operationEntitiesEnumerable.ToArray();
            // only 1 operation has been executed
            operationEntities.Length.Should().Be(1);
            operationEntities.First().Status.Should().Be(OperationStatus.Committed);

            var manufacturersInDb = _mockDatabase.Manufacturers.Get(x => x.Id == manufacturer.Id);
            manufacturersInDb.First().Should().Be(manufacturer);

            // because we have prematurely finished transaction
            _mockDatabase.Autos.Should().BeEmpty();
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
        private class CreateManufacturer : SagaOperationBase<long>
        {
            private readonly MockDatabase _mockDatabase;
            private readonly ITransactionContext _transactionContext;

            public Manufacturer Manufacturer { get; init; }

            public CreateManufacturer(ITransactionContext transactionContext) : base(transactionContext)
            {
                _transactionContext = transactionContext;
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                _mockDatabase.Manufacturers.Add(Manufacturer);

                // saving as rollback data
                RollbackData = Manufacturer.Id;

                // calling premature finish of transaction
                _transactionContext.FinishPrematurely();

                return Task.CompletedTask;
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                _mockDatabase.Manufacturers.Remove(x => x.Id == RollbackData);
                return Task.CompletedTask;
            }
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto))]
        private class CreateAuto : SagaOperationBase<long>
        {
            public Auto Auto { get; init; }

            private readonly MockDatabase _mockDatabase;

            public CreateAuto(ITransactionContext transactionContext) : base(transactionContext)
            {
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                _mockDatabase.Autos.Add(Auto);

                // saving as rollback data
                RollbackData = Auto.Id;

                return Task.CompletedTask;
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                _mockDatabase.Autos.Remove(x => x.Id == RollbackData);
                return Task.CompletedTask;
            }
        }
    }
}
