﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedTransactions.Attributes;
using DistributedTransactions.Builders;
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
        public async Task CreateManufacturer_CreateAutosWithFail_SuccessfullyRollbacks()
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
                .CreateDistributedTransactionExecutor(TransactionContext)
                .UseLogger(GetLogger<DistributedTransactionExecutor>())
                .UseTransactionProvider(TransactionProvider)
                .UseOperationProvider(OperationProvider);

            var createManufacturer = new CreateManufacturer(TransactionContext)
            {
                Manufacturer = manufacturer
            };

            var createAuto = new CreateAuto(TransactionContext)
            {
                Auto = auto
            };

            distributedTransactionExecutor.RegisterOperation(createManufacturer);
            distributedTransactionExecutor.RegisterOperation(createAuto);

            await distributedTransactionExecutor.ExecuteFullTransactionAsync(CancellationToken.None);

            _mockDatabase.Transactions.First().Status.Should().Be(TransactionStatus.FinishedWithRollback.ToString());
            _mockDatabase.Operations.First().Status.Should().Be(OperationStatus.Rollbacked.ToString());

            _mockDatabase.Autos.Should().BeEmpty();
            _mockDatabase.Manufacturers.Should().BeEmpty();
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
        internal class CreateManufacturer : DistributedTransactionOperationBase<long>
        {
            public Manufacturer Manufacturer { get; set; }

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
                _mockDatabase.Manufacturers.RemoveById(RollbackData);
                return Task.CompletedTask;
            }
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto))]
        internal class CreateAuto : DistributedTransactionOperationBase<long>
        {
            public Auto Auto { get; set; }

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