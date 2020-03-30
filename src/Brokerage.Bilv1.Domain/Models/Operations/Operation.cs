using System.Numerics;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;

namespace Brokerage.Bilv1.Domain.Models.Operations
{
    public class Operation
    {
        public long OperationId { get; }
        public DepositWalletKey Key { get; }
        public BigInteger BalanceChange { get; }
        public long Block { get; }

        protected Operation(
            DepositWalletKey key,
            BigInteger balanceChange,
            long block,
            long operationId)
        {
            Key = key;
            BalanceChange = balanceChange; 
            Block = block;
            OperationId = operationId;
        }

        public static Operation Create(DepositWalletKey key, BigInteger balanceChange, long block, long operationId)
        {
            return new Operation(key, balanceChange, block, operationId);
        }
    }
}
