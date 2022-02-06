using System;
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

namespace DistributedTransactions.Tests.Saga
{
    [TestFixture]
    internal class SagaFailedRollbackFlowTests : DistributedTransactionsTestsBase
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
        public async Task CreateManufacturer_CreateAutosWithFail_FailsRollbacks()
        {
            var manufacturer = new Manufacturer
            {
                Id = 1,
                Name = "Audi"
            };

            var sagaExecutor = SagaExecutorBuilder.ValidateAndBuild();

            var createManufacturer = new CreateManufacturer(TransactionContext) { Manufacturer = manufacturer };
            var createAuto = new CreateAuto(TransactionContext);

            sagaExecutor.RegisterOperation(createManufacturer);
            sagaExecutor.RegisterOperation(createAuto);

            await sagaExecutor.ExecuteTransactionAsync(CancellationToken.None);

            // instance status
            sagaExecutor.Status.Should().Be(TransactionStatus.FailedToRollback);
            sagaExecutor.LastOccuredException.Should().NotBeNull();
            sagaExecutor.LastOccuredException.GetType().Should().Be(typeof(NotImplementedException));

            // in database entities
            var transactionIdBeforeRollback = sagaExecutor.TransactionId!.Value;
            var transactionBeforeRollback = await _transactionProvider.GetByTransactionIdAsync(transactionIdBeforeRollback, CancellationToken.None);
            transactionBeforeRollback.Status.Should().Be(TransactionStatus.FailedToRollback);

            var operationsBeforeRollback = await _operationProvider.GetByTransactionIdAsync(transactionIdBeforeRollback, CancellationToken.None);
            operationsBeforeRollback.First().Status.Should().Be(OperationStatus.FailedToRollback);

            _mockDatabase.Manufacturers.Should().NotBeEmpty();
            _mockDatabase.Manufacturers.First().Id.Should().Be(manufacturer.Id);
            _mockDatabase.Manufacturers.First().Name.Should().Be(manufacturer.Name);
            _mockDatabase.Autos.Should().BeEmpty();

            // trying to rollback that saga again and check moving to another status - `FailedToRollback`
            await sagaExecutor.RollbackTransactionAsync(transactionBeforeRollback, CancellationToken.None);

            sagaExecutor.Status.Should().Be(TransactionStatus.FailedToRollback);
            sagaExecutor.LastOccuredException.Should().NotBeNull();
            sagaExecutor.LastOccuredException.GetType().Should().Be(typeof(NotImplementedException));

            var transactionIdAfterRollback = sagaExecutor.TransactionId!.Value;
            var transactionAfterRollback = await _transactionProvider.GetByTransactionIdAsync(transactionIdAfterRollback, CancellationToken.None);
            transactionAfterRollback.Status.Should().Be(TransactionStatus.FailedToRollback);

            var operationsAfterRollback = await _operationProvider.GetByTransactionIdAsync(transactionIdAfterRollback, CancellationToken.None);
            operationsAfterRollback.First().Status.Should().Be(OperationStatus.FailedToRollback);

            _mockDatabase.Manufacturers.Should().NotBeEmpty();
            _mockDatabase.Manufacturers.First().Id.Should().Be(manufacturer.Id);
            _mockDatabase.Manufacturers.First().Name.Should().Be(manufacturer.Name);
            _mockDatabase.Autos.Should().BeEmpty();
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
        private class CreateManufacturer : SagaOperationBase<long>
        {
            public Manufacturer Manufacturer { get; init; }

            private readonly MockDatabase _mockDatabase;

            public CreateManufacturer(ITransactionContext transactionContext) : base(transactionContext)
            {
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                _mockDatabase.Manufacturers.Add(Manufacturer);

                // saving as rollback data
                RollbackData = Manufacturer.Id;

                return Task.CompletedTask;
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto))]
        private class CreateAuto : SagaOperationBase<long>
        {
            private readonly MockDatabase _mockDatabase;

            public CreateAuto(ITransactionContext transactionContext) : base(transactionContext)
            {
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
