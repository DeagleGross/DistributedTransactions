## Distributed Transactions Usage
You can investigate [test project](../tests/DistributedTransactions.Tests/Saga/SagaFlowTests.Success.cs), because it contains sample examples of this API usage

###How to setup this library in your service:
1. add migrations for `distributed_transaction` table and `distributed_transaction_operation` table.
2. add library API in `Startup.cs` using [ServiceCollectionExtensions](../src/DistributedTransactions/Extensions/ServiceCollectionExtensions.cs)

### How to rollback previously failed transactions:
If you need to create a worker-class, that will process failed before transactions, you can use special class [SagaRollbackWorker](../src/DistributedTransactions/Saga/Workers/SagaRollbackWorker.cs).
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

## Principles to follow
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