﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities.Deposits
{
    [Table(name: Tables.DepositFees)]
    public class DepositFeeEntity
    {
        public long DepositId { get; set; }

        public long TransferId { get; set; }

        public DepositEntity DepositEntity { get; set; }

        public long AssetId { get; set; }

        public decimal Amount { get; set; }
    }
}