# Distributed Transactions Library Terminology

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