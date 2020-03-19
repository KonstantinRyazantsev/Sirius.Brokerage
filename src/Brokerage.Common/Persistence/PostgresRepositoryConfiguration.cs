namespace Brokerage.Common.Persistence
{
    public static class PostgresRepositoryConfiguration
    {
        public static string SchemaName { get; } = "brokerage";

        public static string MigrationHistoryTable { get; } = "__EFMigrationsHistory_brokerage";
    }
}
