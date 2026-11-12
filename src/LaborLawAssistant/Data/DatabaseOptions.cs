namespace LaborLawAssistant.Data;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    public const string PostgreSqlProvider = "PostgreSQL";

    public string Provider { get; set; } = "Sqlite";

    public bool MigrateOnStartup { get; set; } = true;

    public bool UsePostgreSql => Provider.Equals(PostgreSqlProvider, StringComparison.OrdinalIgnoreCase);
}
