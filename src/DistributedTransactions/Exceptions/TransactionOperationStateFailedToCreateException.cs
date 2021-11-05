﻿using DistributedTransactions.DAL.Models;
using System;

namespace DistributedTransactions.Exceptions
{
    public class TransactionOperationStateFailedToCreateException : Exception
    {
        public TransactionOperationStateFailedToCreateException(string message) : base(message)
        {
        }

        internal static TransactionOperationStateFailedToCreateException Create(OperationEntity entity)
            => new($"{nameof(OperationEntity)} failed to be created when inserting to database with such parameters: operationId='{entity.Id}', transactionId='{entity.TransactionId}'");
    }
}
