using System.Collections.Generic;
using System.Linq;
using DistributedTransactions.Tests.Mocks.Models;

namespace DistributedTransactions.Tests.Mocks.Database
{
    public static partial class MockDatabase
    {
        private static readonly IList<Manufacturer> _manufacturers = new List<Manufacturer>();

        public static void Add(Manufacturer manufacturer)
        {
            _manufacturers.Add(manufacturer);
        }

        public static void Add(IEnumerable<Manufacturer> manufacturers)
        {
            foreach (var manufacturer in manufacturers)
            {
                Add(manufacturer);
            }
        }

        public static void GetManufacturerById(long id) => _manufacturers.FirstOrDefault(x => x.Id == id);
    }
}
