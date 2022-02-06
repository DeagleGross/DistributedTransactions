using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Exceptions;
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
    internal class SagaValidationTests : DistributedTransactionsTestsBase
    {
        [OneTimeSetUp]
        public void SetupServiceProvider()
        {
            OneTimeSetup();
        }

        [Test]
        public Task RegisterOperation_ThrowsDifferentTransactionTypesPassed()
        {
            var sagaExecutor = SagaExecutorBuilder.ValidateAndBuild();

            var createManufacturer = new CreateManufacturer(TransactionContext);
            var createAuto = new CreateAuto(TransactionContext);

            var registerOperationsAction = new Action(() =>
            {
                sagaExecutor.RegisterOperation(createManufacturer);
                sagaExecutor.RegisterOperation(createAuto);
            });

            sagaExecutor.TransactionId.Should().Be(null);
            sagaExecutor.Status.Should().Be(null);

            registerOperationsAction.Should().Throw<DifferentTransactionTypeValuesLoadedException>();
            return Task.CompletedTask;
        }

        [Test]
        public Task ExecuteFullTransaction_ThrowsNoOperationsRegistered()
        {
            var sagaExecutor = SagaExecutorBuilder.ValidateAndBuild();

            var registerOperationsAction = new Action(() =>
            {
                sagaExecutor.ExecuteTransactionAsync(CancellationToken.None).GetAwaiter().GetResult();
            });

            sagaExecutor.TransactionId.Should().Be(null);
            sagaExecutor.Status.Should().Be(null);

            registerOperationsAction.Should().Throw<NoTransactionOperationsRegisteredException>();
            return Task.CompletedTask;
        }

        [DistributedTransactionOperation("random_type_1", nameof(OperationType.CreateManufacturer))]
        internal class CreateManufacturer : SagaOperationBase<long>
        {
            public Manufacturer Manufacturer { get; set; }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                throw new Exception();
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new Exception();
            }

            public CreateManufacturer(ITransactionContext transactionContext) : base(transactionContext)
            {
            }
        }

        [DistributedTransactionOperation("random_type_2", nameof(OperationType.CreateAuto))]
        internal class CreateAuto : SagaOperationBase<long>
        {
            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                throw new Exception();
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                throw new Exception();
            }

            public CreateAuto(ITransactionContext transactionContext) : base(transactionContext)
            {
            }
        }
    }
}
