using System;
using DistributedTransactions.DAL.Models;
using DistributedTransactions.Models;

namespace DistributedTransactions.Converters
{
    internal static class TransactionConverter
    {
        public static TransactionEntity ToEntity(Transaction transaction) => new()
        {
            Id = transaction.Id,
            TransactionType = transaction.TransactionType,
            Status = transaction.Status.ToString()
        };

        public static Transaction FromEntity(TransactionEntity entity) => new()
        {
            Id = entity.Id,
            TransactionType = entity.TransactionType,
            Status = Enum.Parse<TransactionStatus>(entity.Status)
        };
    }
}
