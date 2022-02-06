# Solution that Library Provides

## User steps
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

## Database Schema

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

## Mid-Transaction Operations

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