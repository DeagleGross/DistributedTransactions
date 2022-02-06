using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Saga.Models.Abstractions;
using DistributedTransactions.Saga.Workers;
using DistributedTransactions.Tests.Base;
using DistributedTransactions.Tests.Mocks;
using DistributedTransactions.Tests.Mocks.Models;
using FluentAssertions;
using NUnit.Framework;

namespace DistributedTransactions.Tests.Saga
{
    [TestFixture]
    internal class SagaRollbackWorkerTests : DistributedTransactionsTestsBase
    {
        private static int _rollbackReferringCount = 1;

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
        public async Task WorkerRollbacksHistoryTransactions_Success()
        {
            var manufacturer = new Manufacturer
            {
                Id = 1,
                Name = "Audi"
            };

            var sagaExecutorBuilder = SagaExecutorBuilder;
            var sagaExecutor = sagaExecutorBuilder.ValidateAndBuild();

            var createManufacturer = new CreateManufacturer(TransactionContext)
            {
                Manufacturer = manufacturer
            };

            var createAuto = new CreateAuto(TransactionContext)
            {
            };

            sagaExecutor.RegisterOperation(createManufacturer);
            sagaExecutor.RegisterOperation(createAuto);

            await sagaExecutor.ExecuteTransactionAsync(CancellationToken.None);
            var transactionIdBeforeWorker = sagaExecutor.TransactionId!.Value;

            // first time transaction fails to rollback
            var transactionBeforeWorker = await _transactionProvider.GetByTransactionIdAsync(transactionIdBeforeWorker, CancellationToken.None);
            transactionBeforeWorker.Status.Should().Be(TransactionStatus.FailedToRollback);
            var operationBeforeWorker = await _operationProvider.GetByTransactionIdAsync(transactionIdBeforeWorker, CancellationToken.None);
            operationBeforeWorker.First().Status.Should().Be(OperationStatus.FailedToRollback);
            _mockDatabase.Manufacturers.Should().NotBeEmpty();

            var rollbackerWorker = new SagaRollbackWorker(sagaExecutorBuilder, GetLogger<SagaRollbackWorker>());
            await rollbackerWorker.RollbackHistoryTransactions(CancellationToken.None);

            // afterRollback
            var transactionAfterWorker = await _transactionProvider.GetByTransactionIdAsync(transactionIdBeforeWorker, CancellationToken.None);
            transactionAfterWorker.Status.Should().Be(TransactionStatus.FinishedWithRollback);
            var operationAfterWorker = await _operationProvider.GetByTransactionIdAsync(transactionIdBeforeWorker, CancellationToken.None);
            operationAfterWorker.First().Status.Should().Be(OperationStatus.Rollbacked);
            _mockDatabase.Manufacturers.Should().BeEmpty();
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
                // first time throwing exception, so that it will not be rollback-ed
                if (_rollbackReferringCount == 1)
                {
                    _rollbackReferringCount++;
                    throw new Exception();
                }

                _mockDatabase.Manufacturers.Remove(x => x.Id == RollbackData);
                return Task.CompletedTask;
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
