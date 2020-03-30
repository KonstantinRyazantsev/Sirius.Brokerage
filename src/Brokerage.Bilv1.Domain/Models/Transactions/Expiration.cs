using System;

namespace Brokerage.Bilv1.Domain.Models.Transactions
{
    public sealed class Expiration
    {
        public DateTime AfterDateTime { get; set; }
        public long AfterBlock { get; set; }
    }
}
