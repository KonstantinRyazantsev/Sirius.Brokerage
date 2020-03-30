namespace Brokerage.Bilv1.Repositories
{
    public class PostgresBilV1RepositoryConfiguration
    {
        public static string SchemaName { get; } = "brokerage_bil_v1";

        public static string MigrationHistoryTable { get; } = "__EFMigrationsHistory_brokerage_bil_v1";
    }
}
