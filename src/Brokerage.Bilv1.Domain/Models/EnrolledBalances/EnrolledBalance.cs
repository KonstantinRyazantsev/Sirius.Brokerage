using System.Numerics;

namespace Brokerage.Bilv1.Domain.Models.EnrolledBalances
{
    public sealed class EnrolledBalance
    {
        public DepositWalletKey Key { get; }
        public BigInteger Balance { get; set; }
        public long Block { get; set; }

        private EnrolledBalance(
            DepositWalletKey key,
            BigInteger balance,
            long block)
        {
            Balance = balance;
            Key = key;
            Block = block;
        }

        public static EnrolledBalance Create(DepositWalletKey key, BigInteger balance, long block)
        {
            return new EnrolledBalance(key, balance, block);
        }
    }
}
