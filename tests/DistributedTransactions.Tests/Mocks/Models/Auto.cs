namespace DistributedTransactions.Tests.Mocks.Models
{
    public class Auto : IMockModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long ManufacturerId { get; set; }
    }
}
