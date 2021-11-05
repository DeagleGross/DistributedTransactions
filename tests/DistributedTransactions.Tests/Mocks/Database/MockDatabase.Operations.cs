using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.DAL.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public static partial class MockDatabase
    {
        private static readonly IList<OperationEntity> _operationStatusEntities = new List<OperationEntity>();

        public static void Add(OperationEntity operationEntity)
        {
            _operationStatusEntities.Add(operationEntity);
        }

        public static void Add(IEnumerable<OperationEntity> operationStatusEntities)
        {
            foreach (var operationStatusEntity in operationStatusEntities)
            {
                Add(operationStatusEntity);
            }
        }

        public static void GetOperationStatusEntityById(long id) => _operationStatusEntities.FirstOrDefault(x => x.Id == id);

    }
}
