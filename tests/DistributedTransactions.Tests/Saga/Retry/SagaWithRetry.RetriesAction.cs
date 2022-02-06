using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Models.Settings;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Models.Abstractions;
using DistributedTransactions.Tests.Base;
using DistributedTransactions.Tests.Mocks;
using DistributedTransactions.Tests.Mocks.Models;
using FluentAssertions;
using NUnit.Framework;

namespace DistributedTransactions.Tests.Saga.Retry
{
    [TestFixture]
    internal class SagaWithRetryRetriesTests : DistributedTransactionsTestsBase
    {
        private ITransactionProvider _transactionProvider;
        private IOperationProvider _operationProvider;
        private MockDatabase _mockDatabase;

        [OneTimeSetUp]
        public void SetupServiceProvider()
        {
            OneTimeSetup(new RetryPolicy
            {
                IsTurnedOn = true,
                RollbackRetryCount = 3,
                CommitRetryCount = 3
            });
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
        public async Task CreateManufacturer_CreateAutosWithFail_InvalidRollbacksDefined_SavesStateCorrectly()
        {
            var manufacturer = new Manufacturer { Id = 1, Name = "Audi" };

            var createManufacturer = new CreateManufacturer(TransactionContext) { Manufacturer = manufacturer };
            var createAuto = new CreateAuto(TransactionContext) { };

            var sagaExecutor = SagaExecutorBuilder.ValidateAndBuild();
            sagaExecutor.RegisterOperation(createManufacturer);
            sagaExecutor.RegisterOperation(createAuto);

            await sagaExecutor.ExecuteTransactionAsync(CancellationToken.None);

            // instance status
            sagaExecutor.Status.Should().Be(TransactionStatus.Failed);
            sagaExecutor.LastOccuredException.Should().NotBeNull();
            sagaExecutor.LastOccuredException.GetType().Should().Be(typeof(NotImplementedException));

            // in database entities
            var transactionId = sagaExecutor.TransactionId!.Value;
            var transaction = await _transactionProvider.GetByTransactionIdAsync(transactionId, CancellationToken.None);
            transaction.Status.Should().Be(TransactionStatus.Failed);

            var operations = await _operationProvider.GetByTransactionIdAsync(transactionId, CancellationToken.None);
            operations.Should().BeNullOrEmpty();

            _mockDatabase.Manufacturers.Should().BeEmpty();
            _mockDatabase.Autos.Should().BeEmpty();
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
        private class CreateManufacturer : SagaOperationBase<long>
        {
            public Manufacturer Manufacturer { get; init; }

            private readonly MockDatabase _mockDatabase;

            private int _commitCount = 0;

            public CreateManufacturer(ITransactionContext transactionContext) : base(transactionContext)
            {
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                if (_commitCount < 5)
                {
                    _commitCount++;
                    throw new NotImplementedException();
                }

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
