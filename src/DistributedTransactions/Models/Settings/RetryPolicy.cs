using System;

namespace DistributedTransactions.Models.Settings
{
    public class RetryPolicy
    {
        public bool IsTurnedOn { get; set; }

        public int CommitRetryCount { get; set; }

        public int RollbackRetryCount { get; set; }

        internal int GetRetryCount(OperationActionType operationActionType) => operationActionType switch
        {
            OperationActionType.Commit => CommitRetryCount,
            OperationActionType.Rollback => RollbackRetryCount,
            _ => throw new ArgumentOutOfRangeException(nameof(operationActionType), operationActionType, null)
        };
    }
}
