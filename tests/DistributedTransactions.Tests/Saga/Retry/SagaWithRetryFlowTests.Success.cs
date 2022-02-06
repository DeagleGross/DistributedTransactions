﻿using System;
using System.Linq;
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
    internal class SagaWithRetrySuccessFlowTests : DistributedTransactionsTestsBase
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
            sagaExecutor.Status.Should().Be(TransactionStatus.FinishedCorrectly);
            sagaExecutor.LastOccuredException.GetType().Should().Be(typeof(NotImplementedException));

            // in database entities
            var transactionId = sagaExecutor.TransactionId!.Value;
            var transaction = await _transactionProvider.GetByTransactionIdAsync(transactionId, CancellationToken.None);
            transaction.Status.Should().Be(TransactionStatus.FinishedCorrectly);

            var operationEntities = await _operationProvider.GetByTransactionIdAsync(transactionId, CancellationToken.None);
            operationEntities.First().Status.Should().Be(OperationStatus.Committed);

            var manufacturersInDb = _mockDatabase.Manufacturers.Get(x => x.Id == manufacturer.Id);
            manufacturersInDb.First().Should().Be(manufacturer);

            var autosInDb = _mockDatabase.Autos.Get(x => x.Id == auto.Id);
            autosInDb.First().Should().Be(auto);
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
        private class CreateManufacturer : SagaOperationBase<long>
        {
            public Manufacturer Manufacturer { get; init; }

            private readonly MockDatabase _mockDatabase;

            private int _commitCount = 0;
            private int _rollbackCount = 0;

            public CreateManufacturer(ITransactionContext transactionContext) : base(transactionContext)
            {
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                if (_commitCount < 3)
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
                if (_rollbackCount < 3)
                {
                    _rollbackCount++;
                    throw new NotImplementedException();
                }

                _mockDatabase.Manufacturers.Remove(x => x.Id == RollbackData);
                return Task.CompletedTask;
            }
        }

        [DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateAuto))]
        private class CreateAuto : SagaOperationBase<long>
        {
            public Auto Auto { get; init; }

            private readonly MockDatabase _mockDatabase;

            private int _commitCount = 0;
            private int _rollbackCount = 0;

            public CreateAuto(ITransactionContext transactionContext) : base(transactionContext)
            {
                _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
            }

            public override Task CommitAsync(CancellationToken cancellationToken)
            {
                if (_commitCount < 3)
                {
                    _commitCount++;
                    throw new NotImplementedException();
                }

                _mockDatabase.Autos.Add(Auto);

                // saving as rollback data
                RollbackData = Auto.Id;

                return Task.CompletedTask;
            }

            public override Task RollbackAsync(CancellationToken cancellationToken)
            {
                if (_rollbackCount < 3)
                {
                    _rollbackCount++;
                    throw new NotImplementedException();
                }

                _mockDatabase.Autos.Remove(x => x.Id == RollbackData);
                return Task.CompletedTask;
            }
        }
    }
}
