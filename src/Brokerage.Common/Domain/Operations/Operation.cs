using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Operations
{
    public sealed class Operation
    {
        private Operation(
            long id,
            OperationType type,
            IReadOnlyCollection<Unit> actualFees,
            IReadOnlyCollection<Unit> expectedFees,
            uint version)
        {
            Id = id;
            Type = type;
            ActualFees = actualFees ?? Array.Empty<Unit>();
            ExpectedFees = expectedFees ?? Array.Empty<Unit>();
            Version = version;
        }

        public long Id { get; }
        public OperationType Type { get; }
        public IReadOnlyCollection<Unit> ActualFees { get; private set; }
        public IReadOnlyCollection<Unit> ExpectedFees { get; private set; }
        public uint Version { get; }

        public static Operation Create(
            long id,
            OperationType type)
        {
            return new Operation(id, type, null, null, 0);
        }

        public void AddExpectedFees(IReadOnlyCollection<Unit> expectedFees)
        {
            ExpectedFees = expectedFees;
        }

        public void AddActualFees(IReadOnlyCollection<Unit> actualFees)
        {
            ActualFees = actualFees;
        }

        public static Operation Restore(
                        long id,
                        OperationType type,
                        IReadOnlyCollection<Unit> actualFees,
                        IReadOnlyCollection<Unit> expectedFees,
                        uint version)
        {
            return new Operation(id, type, actualFees, expectedFees, version);
        }
    }
}
