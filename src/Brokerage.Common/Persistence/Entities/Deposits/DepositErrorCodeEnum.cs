namespace Brokerage.Common.Persistence.Entities.Deposits
{
    public enum DepositErrorCodeEnum
    {
        TechnicalProblem = 0,
        NotEnoughBalance = 1,
        InvalidDestinationAddress = 2,
        DestinationTagRequired = 3,
        AmountIsTooSmall = 4
    }
}
