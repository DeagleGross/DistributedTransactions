using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Builders;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Executors;
using DistributedTransactions.Models.Abstractions;
using DistributedTransactions.Providers.Abstractions;
using DistributedTransactions.Tests.Base;
using DistributedTransactions.Tests.Mocks;
using DistributedTransactions.Tests.Mocks.Database;
using DistributedTransactions.Tests.Mocks.Models;
using FluentAssertions;
using NUnit.Framework;

namespace DistributedTransactions.Tests
{
    [TestFixture]
    internal class DistributedTransactionValidationTests : DistributedTransactionsTestsBase
    {
        private MockDatabase _mockDatabase;

        [SetUp]
        public void Setup()
        {
            _mockDatabase = MockDatabase;
            _mockDatabase.ClearAll();
        }

        [Test]
        public async Task RegisterOperation_ThrowsDifferentTransactionTypesPassed()
        {
            var distributedTransactionExecutor = DistributedTransactionExecutorBuilder
                .CreateDistributedTransactionExecutor(TransactionContext)
                .UseLogger(GetLogger<DistributedTransactionExecutor>())
                .UseTransactionProvider(TransactionProvider)
                .UseOperationProvider(OperationProvider);

            var createManufacturer = new CreateManufacturer(TransactionContext);
            var createAuto = new CreateAuto(TransactionContext);

            var registerOperationsAction = new Action(() =>
            {
                distributedTransactionExecutor.RegisterOperation(createManufacturer);
                distributedTransactionExecutor.RegisterOperation(createAuto);
            });

            registerOperationsAction.Should().Throw<DifferentTransactionTypeValuesLoadedException>();
        }

        [Test]
        public async Task ExecuteFullTransaction_ThrowsNoOperationsRegistered()
        {
            var distributedTransactionExecutor = DistributedTransactionExecutorBuilder
                .CreateDistributedTransactionExecutor(TransactionContext)
                .UseLogger(GetLogger<DistributedTransactionExecutor>())
                .UseTransactionProvider(TransactionProvider)
                .UseOperationProvider(OperationProvider);

            var registerOperationsAction = new Action(() =>
            {
                distributedTransactionExecutor.ExecuteFullTransactionAsync(CancellationToken.None).GetAwaiter().GetResult();
            });

            registerOperationsAction.Should().Throw<NoTransactionOperationsRegisteredException>();
        }

        [DistributedTransactionOperation("random_type_1", nameof(OperationType.CreateManufacturer))]
        internal class CreateManufacturer : DistributedTransactionOperationBase<long>
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
        internal class CreateAuto : DistributedTransactionOperationBase<long>
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
