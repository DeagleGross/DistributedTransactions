using System;
using DistributedTransactions.Exceptions;
using DistributedTransactions.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace DistributedTransactions.Tests
{
    [TestFixture]
    public class ServiceTransactionContextTests : DistributedTransactionsTestsBase
    {
        [OneTimeSetUp]
        public void SetupServiceProvider()
        {
            OneTimeSetup();
        }

        [SetUp]
        public void Setup()
        {
            // clearing transaction context before every test, so that keys of added data are not colliding
            TransactionContext.ClearInterTransactionalData();
        }

        [Test]
        public void TransactionContext_GetNotValidType_ThrowsException()
        {
            long data = 1;
            var transactionContext = TransactionContext;
            transactionContext.SaveInterTransactionalData("a", data);

            var getDataAction = new Action(() =>
            {
                long longId = transactionContext.GetInterTransactionalData<int>("a");
            });

            getDataAction.Should().Throw<InterTransactionalDataInvalidCastException>();
        }

        [Test]
        public void TransactionContext_GetLong_Success()
        {
            long data = 1;
            var transactionContext = TransactionContext;
            transactionContext.SaveInterTransactionalData("a", data);
            var longId = transactionContext.GetInterTransactionalData<long>("a");
            longId.Should().Be(1);
        }

        [Test]
        public void TransactionContext_LoadsData_Int_SavesData_Success()
        {
            int data = 1;
            var transactionContext = TransactionContext;
            transactionContext.SaveInterTransactionalData("a", data);
            var intId = transactionContext.GetInterTransactionalData<int>("a");
            intId.Should().Be(data);
        }

        [Test]
        public void TransactionContext_LoadsData_CustomType_SavesData_Success()
        {
            var person = new Person { Id = 1, Name = "jack" };
            var transactionContext = TransactionContext;
            transactionContext.SaveInterTransactionalData("a", person);
            var personFomSavedData = transactionContext.GetInterTransactionalData<Person>("a");
            personFomSavedData.Should().Be(person);
        }

        [Test]
        public void TransactionContext_LoadsData_String_SavesData_Success()
        {
            string data = "hello world";
            var transactionContext = TransactionContext;
            transactionContext.SaveInterTransactionalData("a", data);
            var stringValue = transactionContext.GetInterTransactionalData<string>("a");
            stringValue.Should().Be(data);
        }

        private class Person
        {
            public long Id { get; init; }

            public string Name { get; init; }
        }
    }
}
