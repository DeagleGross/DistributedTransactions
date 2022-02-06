namespace DistributedTransactions.Models.Settings
{
    public class DistributedTransactionServiceOwnerInfo
    {
        public string ServiceName { get; }

        public DistributedTransactionServiceOwnerInfo(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}
