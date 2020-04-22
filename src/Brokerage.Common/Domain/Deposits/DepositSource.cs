namespace Brokerage.Common.Domain.Deposits
{
    public class DepositSource
    {
        public DepositSource(string address, decimal amount)
        {
            Address = address;
            Amount = amount;
        }
        public string Address { get; }

        public decimal Amount { get; }
    }
}
