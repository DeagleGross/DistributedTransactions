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

namespace DistributedTransactions.Tests.Saga.Staged
{
    [TestFixture]
    internal class SagaStagedFailedRollbackFlowTests : DistributedTransactionsTestsBase
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
            var createAuto1 = new CreateAuto1(TransactionContext);
            var createAuto2 = new CreateAuto2(TransactionContext);

            sagaExecutor.RegisterOperation(createManufacturer);
            sagaExecutor.RegisterOperation(createAuto1);
            sagaExecutor.RegisterOperation(createAuto2);

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
        }

        [DistributedTransactionOperation(
            transactionType: nameof(TransactionType.CreateManufacturerWithAuto),
            operationType: nameof(OperationType.CreateManufacturer),
            ExecutionStage = 1)]
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

        [DistributedTransactionOperation(
            transactionType: nameof(TransactionType.CreateManufacturerWithAuto),
            operationType: nameof(OperationType.CreateAutoSecond),
            ExecutionStage = 2)]
        private class CreateAuto1 : SagaOperationBase<long>
        {
            private readonly MockDatabase _mockDatabase;

            public CreateAuto1(ITransactionContext transactionContext) : base(transactionContext)
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

        [DistributedTransactionOperation(
            transactionType: nameof(TransactionType.CreateManufacturerWithAuto),
            operationType: nameof(OperationType.CreateAutoSecond),
            ExecutionStage = 2)]
        private class CreateAuto2 : SagaOperationBase<long>
        {
            private readonly MockDatabase _mockDatabase;

            public CreateAuto2(ITransactionContext transactionContext) : base(transactionContext)
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
