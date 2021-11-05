using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public static partial class MockDatabase
    {
        private static readonly IList<Auto> _autos = new List<Auto>();

        public static void Add(Auto auto)
        {
            _autos.Add(auto);
        }

        public static void Add(IEnumerable<Auto> autos)
        {
            foreach (var auto in autos)
            {
                Add(auto);
            }
        }

        public static void GetAutoById(long id) => _autos.FirstOrDefault(x => x.Id == id);
    }
}
