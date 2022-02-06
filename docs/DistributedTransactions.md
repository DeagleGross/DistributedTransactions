# Distributed Transactions
C# stateful orchestrated SAGA implementation of distributed transactions for any user-defined operations.
**Usage** section can be found at the end of this documentation, but it is preferred to read this for better understanding.

## How SAGA works
[You can read about SAGA pattern here](https://medium.com/@ijayakantha/microservices-the-saga-pattern-for-distributed-transactions-c489d0ac0247)

## Terminology
**Operation** - an atomic action that can finish successfully (expected) or unsuccessfully.
- Because of library is about distributed functionality, operation is an action that is aimed to add\update\delete data at another microservice.
- Operation interface is easy to understand - it encapsulates a `commit` and `rollback` methods.
```c#
Task CommitAsync(CancellationToken cancellationToken);

Task RollbackAsync(CancellationToken cancellationToken);
```
- Operation can exist in three states:
```
public enum OperationStatus
{
    Committed,
    NeedsToRollback,
    FailedToRollback
    Rollbacked
}
```

**Transaction** - a set of operations. They are grouped by the same *transaction_type*.
Transaction can exist in these states:
```c#
public enum TransactionStatus
{
    // successful flow
    Created,
    FinishedCorrectly,

    // finally successful flow
    FinishedWithRollback,

    // unsuccessful flow
    NeedsToRollback,
    FailedToRollback
    Failed
}
```

- `Created` stands for a created transaction, but no operations were executed so far.
- `FinishedCorrectly` means that transaction has executed all of operations successfully.
- `NeedsToRollback` transaction stands for a transaction, to which some operations are bound, which are in the same state.
   So we definitely can try to query `OperationEntity[]` from a database and try to rollback them.
- `FailedToRollback` transaction means it was in `NeedsToRollback` state, but failed to rollback.
   You need to be cautious maybe you have to manually rollback such operations because they can infinitely try to rollback and never ever succeed.
- `FinishedWithRollback` means transaction has failed before N times, but finally at some moment it succeeded to rollback data
- `Failed` means it was created, but the first operation has failed, so we could not rollback it, because no data needs to be rollback-ed.
  However there is no reason to leave transaction in `Created` state, because we know it has failed.


## The workflow
For making a transaction, user has to follow these steps:
- create a class-type, which inherits `SagaOperationBase<TRollbackData>`. This class stands for an operation in a transaction.
- implement `CommitAsync()` and `RollbackAsync()` methods
- using `SagaExecutorBuilder` create an instance of `SagaExecutor`
- register all of operations that developer wants to commit in a single transaction using `SagaExecutor.RegisterOperation()` method
- execute transaction using `SagaExecutor.ExecuteTransactionAsync()` method

From now on everything is done by transaction executor and user doesn't need to worry about anything.
Here are possible outcomes:
- if all operations in a transaction succeed, all operations are marked as `Comitted` and transaction is in a `FinishedCorrectly` status. There is nothing we can do about transaction anymore.
- if first operation has failed, we don't even need to rollback anything. So transaction is created, but no operations are bound to that transaction. Again we don't do anything (just placing it in `Failed` state in database)
- if first operation has succeeded, but then any operation has failed, we mark every operation with `NeedsToRollback` status. Then we start rollback-ing all of them till the one that has failed.

## Architecture
This library is stateful, so it saves statuses of committed and rollback-ed operations in a database.
There are two tables that contain all of the information about transaction operations.

`distributed_transaction` table has unique `id`, a text identifier for a `transaction_type` (i.e. it could be *CreateLogisticOrderAndHandoverTransactionType*) and a status
```postgresql
CREATE TABLE distributed_transaction
(
     id                 BIGSERIAL   NOT NULL CONSTRAINT distributed_transaction_id_pk PRIMARY KEY,
     transaction_type   TEXT        NOT NULL,
     status             TEXT        NOT NULL
);
```

`distributed_transaction_operations` table stands for an operation. It has a `transaction_id` as an foreign key to `distributed_transaction` table.
Also operation saves `rollback_data_type`, `rollback_data` and `executor_type`. It is very important to understand meaning of these fields.

```postgresql
CREATE TABLE distributed_transaction_operation
(
     id                            BIGSERIAL   NOT NULL PRIMARY KEY,
     transaction_id                BIGINT      NOT NULL,
     operation_type                TEXT        NOT NULL,
     rollback_priority             INT,
     execution_stage               INT,
     rollback_data_type            TEXT        NOT NULL,
     executor_type                 TEXT        NOT NULL,
     rollback_data                 TEXT        NOT NULL,
     status                        TEXT        NOT NULL
);
```

- `rollback_data` is a serialized string value of data needed for `RollbackAsync()` method.
- `rollback_data_type` is a `System.Type` value saved in a string representation, so that we can use reflection and recreate data programmatically.
- `executor_type` is a user-defined type of operation. We need to save that type to recreate class using reflection.

When transaction succeeds, there is not much we need to do.
But when transaction is in rollback stage, we are retrieving `rollback_data` and `rollback_data_type` from database, creating an instance using `Activator.CreateInstance` and executing `RollbackAsync`.

There is one trouble left - we need to somehow inject services, httpClients and other objects to operation instance,
so that we can i.e. send a delete request to another microservice for rollback-ing the creation of some items.

So there is a special interface for injecting any services.
```c#
public interface ITransactionContext
{
    T GetRequiredService<T>();
}
```

There is a special implementation `ServiceTransactionContext`, that delegates an implementation to `IServiceProvider` of your ASP.NET service.
So that user can use any service registered by the mechanism *Dependency Injection*

## Usage
You can investigate [test project](../../tests/Aer.Platform.DistributedTransaction.Tests/SAGA/SagaFlowTests.Success.cs), because it contains sample examples of this API usage

###How to setup this library in your service:
1. add migrations for [distributed_transaction](../../tests/Aer.Platform.Tests.Common/Migrations/0002_AddDistributedTransactionsOperationTable.cs) and [distributed_transaction_operation](../../tests/Aer.Platform.Tests.Common/Migrations/0003_AddDistributedTransactionsTransactionTable.cs) as done in the test project.
   This can be done creating two classes of migrations and inheriting them from [AddDistributedTransactionsOperationMigration](../Aer.Platform.Migrations/DistributedTransactionsMigration.Operation.cs) and [AddDistributedTransactionsTransactionMigration](../Aer.Platform.Migrations/DistributedTransactionsMigration.Transaction.cs)
2. add postgresql type definition for `distributed_transaction_v1` and `distributed_transaction_operation_v1` types.
   This can be done using extension method [INpgsqlTypeMapper.MapDistributedTransactionEntities()](../Aer.Platform.DistributedTransactions.DAL/Extensions/PostgreSqlTypeMapperExtensions.cs)
3. add library API in `Startup.cs` using [ServiceCollectionExtensions](./Extensions/ServiceCollectionExtensions.cs)

###How to rollback previously failed transactions:
If you need to create a worker-class, that will process failed before transactions, you can use special class [SagaRollbackWorker](./Saga/Workers/SagaRollbackWorker.cs).
It retrieves all transactions with `NeedsToRollback` status, goes in a loop of that transactions and executes `RollbackAsync()` method of every transaction.
You can call `SagaRollbackWorker.RollbackHistoryTransactions()` method to implement such a logic.

###How to create user-defined operations and executing transaction:
Let's assume we have 2 services - `manufacturer_service` and `auto_service`.
And we need to create `A1`, `A3` and `A5` models of `Audi` manufacturer.
We can image `auto` model has a foreign key connection to `manufacturer` model.

0. It is recommended to do it in such a way - firstly, define a separate class that holds all of the API for transaction
```c#
public class MyTransactionExecutor
{
    private readonly SagaExecutor _sagaExecutor;
    private readonly ITransactionContext _transactionContext;

    public MyTransactionExecutor(SagaExecutorBuilder sagaExecutorBuilder)
    {
        _sagaExecutor = sagaExecutorBuilder.ValidateAndBuild();
        _transactionContext = _sagaExecutor.TransactionContext;
    }

    // and other stuff could be defined here
    // so you won't confuse other classes with exposing API
}
```

1. Then let's create an operation `CreateManufacturer` - it will be the first operation executed in a transaction (view the comments for explanations):
```c#
// attribute of an operation - it is required.
// first parameter is a transaction_type, the second one is operation_type.
[DistributedTransactionOperation(nameof(TransactionType.CreateManufacturerWithAuto), nameof(OperationType.CreateManufacturer))]
// T generic parameter of base class is a type of `RollbackData` property, stored in a parent type.
private class CreateManufacturer : SagaOperationBase<long>
{
    // a model we want to create
    public Manufacturer Manufacturer { get; init; }

    private readonly ITransactionContext _transactionContext;

    // some infrastructure instance we need for performing an operation.
    // it can be an httpClient, if a transaction is performed across several microservices
    private readonly MockDatabase _mockDatabase;

    // it is required to leave a single constructor of type with a single parameter of `ITransactionContext`
    public CreateManufacturer(ITransactionContext transactionContext) : base(transactionContext)
    {
        _transactionContext = transactionContext;
        _mockDatabase = transactionContext.GetRequiredService<MockDatabase>();
    }

    // implement `CommitAsync` in a style
    // i.e. save a manufacturer, then save rollback_data to a property of a base class
    // IT IS IMPORTANT TO SAVE DATA NEEDED FOR ROLLBACK !!!
    public override Task CommitAsync(CancellationToken cancellationToken)
    {
        _mockDatabase.Manufacturers.Add(Manufacturer);

        // saving as rollback data
        RollbackData = Manufacturer.Id;

        // if u need created Manufacturer id in the next operation.commit method of a transaction
        // then u can save it in a transaction context in such a manner:
        _transactionContext.SaveInterTransactionalData("manufacturerId", Manufacturer.Id);

        return Task.CompletedTask;
    }

    // implement `RollbackAsync` in a style
    // use `RollbackData` saved before and remove created model
    public override Task RollbackAsync(CancellationToken cancellationToken)
    {
        _mockDatabase.Manufacturers.Remove(x => x.Id == RollbackData);
        return Task.CompletedTask;
    }
}
```

2. We can now use that type in our workflow
```c#
var createManufacturer = new CreateManufacturer(_transactionContext)
{
    Manufacturer = manufacturer
};
```

3. Create another type - `CreateAuto`. Example can be found in test project.
Also don't forget we can access saved data from previous operation in a transaction using `TransactionContext` type.
*In example*:
```c#
var sellerCodeIdMap = _transactionContext.GetInterTransactionalData<IReadOnlyDictionary<string, long>>("sellerCodeIdMap");
```

4. Create an instance of that type also
```c#
var createAuto = new CreateAuto(_transactionContext)
{
    Auto = auto
};
```

5. Create an instance of `SagaExecutor` using builder (*Builder can be resolved using DI)*:
```c#
sagaExecutor = sagaExecutorBuilder.ValidateAndBuild();
```

6. Register operations, created before
```c#
sagaExecutor.RegisterOperation(createManufacturer);
sagaExecutor.RegisterOperation(createAuto);
```

**Make sure to register them in the same order you want them to be committed!**

7. Call execution of transaction
```c#
await sagaExecutor.ExecuteTransactionAsync(CancellationToken.None);
```

8. Your can use `sagaExecutor.TransactionId` property to get an id of transaction.
   `sagaExecutor.Status` can be used for figuring out in which status saga has finished.
   `sagaExecutor.LastOccuredException` can tell you about last exception that has occured and was caught during execution.
   You can easily throw it back to controller so that user knows the real exception type.
```c#
var transactionId = sagaExecutor.TransactionId;
var transactionStatus = sagaExecutor.Status;
var exception = sagaExecutor.LastOccuredException;
```

## Main Usage principles
- For any operation leave base constructor as it is. Don't add any of the parameters - otherwise your operation could not be constructed programmatically using reflection and rollback would not be possible.
- If you need to access saved in memory inter-transaction data, please, don't do it in constructor. Do it as late as possible - you will ever need it only for `CommitAsync` method. So do it there:
```c#
public override Task CommitAsync(CancellationToken cancellationToken)
{
    // getting data
    var data = _transactionContext.GetInterTransactionalData<PersonData>("personData");

    // your code goes here
    return Task.CompletedTask;
}
```

## Premature Transaction Finishing
Sometimes you need to stop execution of operations in a transaction.
I.e. you have tried to create an entity in external microservice, but it is already there.
You have received an error, however you understand somehow (for example you can determine it receiving HTTP response status)
that it is absolutely ok and you have nothing left to do.

So, you want to stop execution of a transaction - this is what `ITransactionContext.FinishPrematurely()` stands for!
You can use it in any commit method of any operation:
```c#
public override Task CommitAsync(CancellationToken cancellationToken)
{
    // you logic here
    // await ...

    // calling premature finish of transaction
    _transactionContext.FinishPrematurely();
}
```

## Other Transaction and Operation settings
Most of other settings could be set up using `[DistributedTransactionOperation]` attribute parameters.
Please, explore parameters to understand the possibilities.

### Operation Rollback Priority
**!Attention!** Important thing is that order you register operations in a `SagaExecutor` is the same order operations are committed.
If you want to change *commiting* order, just change order of registration.

However you can not really control the rollback priority - this is what `RollbackPriority` parameter stands for.
The bigger value of `RollbackPriority` is, the latter operation rollback will be executed.

This can be specified as a named argument of an attribute:
```c#
[DistributedTransactionOperation(
            transactionType: "my-transaction",
            operationType: "my-operation",
            RollbackPriority = 1)]
```

Please, don't set it as `0` - it will not be set explicitly, because it is a default value.
It will be identified as *not passed parameter* (== null).
This is an implementation detail due to attribute parameter restrictions. [You can read about it here](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/attributes#positional-and-named-parameters)

Operations are selected using basic *SQL SELECT*:
```sql
select * from distributed_transaction_operation
ORDER BY rollback_priority;
```

### Operation Parallel Committing
Imagine you have a transaction with 6 operations inside, but you can not lose time and commit some of operations at the same time.

At this time you can create **stages** - different steps in committing process.
And you can assign operations to different stages. It can be done using named parameter `ExecutionStage` of an `[DistributedTransactionOperation]`.

This can be specified as a named argument of an attribute:
```c#
[DistributedTransactionOperation(
            transactionType: "my-transaction",
            operationType: "my-operation",
            ExecutionStage = 1)]
```

Please, don't set it as `0` - it will not be set explicitly, because it is a default value.
It will be identified as *not passed parameter* (== null).
This is an implementation detail due to attribute parameter restrictions. [You can read about it here](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/attributes#positional-and-named-parameters)

Below is the diagram, where there are 6 operations and 3 stages:
- №1 and №2 have the same *execution_stage* value
- №3 have the another unique *execution_stage* value
- №4, №5, №6 have again the same *execution_stage* value

The lower *execution_stage* is, the earlier it will be executed.
![saga staged execution plan](../docs/res/saga_staged_execution_plan.jpg)

## Retries
Surely, there are some actions that can fail on the first execution and then succeed.
So here comes the retry mechanism - it is implemented using [Polly](https://github.com/App-vNext/Polly).

**By default retries are turned OFF**!

You can setup default settings passing a special parameter to serviceCollection extension method:
```c#
services.AddSaga(retryPolicy: new RetryPolicy
{
    IsTurnedOn = true, // turns retries on and off
    RollbackRetryCount = 3, // amount of retries for any rollback method
    CommitRetryCount = 3 // amount of retries for any commit method
})
```

But if you want to change retry policy specifically for single transaction, you can use [DistributedTransactionSettings](./Models/Settings/DistributedTransactionSettings.cs) class and set your own retry policy. Then you need to create a builder by yourself:
```c#
return new SagaExecutorBuilder(distributedTransactionSettings)
    .AddLogger(logger)
    .AddTransactionContext(transactionContext)
    .AddOperationProvider(operationProvider)
    .AddTransactionProvider(transactionProvider)
    .AddMetricsSender(metricsSender);
```

Then you can build a `sagaExecutor` now and that's all.
By now settings specific retry policy on a single operation in a transaction is not supported.

## Metrics
If you are using `UsePlatform` method in a `Program.cs` main method of your service (and you do), you are adding `Aer.Platform.Metrics` library
and you can send metrics to a Prometheus server. `DistributedTransactions` library gets your service name and sends metrics when any transaction or operation changes it status.
Afterwards you can find metrics of your service working with distributed transactions.
