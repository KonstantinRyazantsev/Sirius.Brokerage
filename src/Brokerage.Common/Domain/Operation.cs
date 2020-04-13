namespace Brokerage.Common.Domain
{
    public sealed class Operation
    {
        public Operation(long id, OperationType type)
        {
            Id = id;
            Type = type;
        }

        public long Id { get; }
        public OperationType Type { get; }
    }
}