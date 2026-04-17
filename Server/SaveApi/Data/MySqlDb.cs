using MySqlConnector;

namespace SaveApi.Data;

public sealed class MySqlDb
{
    private readonly string _connectionString;

    public MySqlDb(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' nao configurada.");
    }

    public MySqlConnection CreateConnection() => new(_connectionString);
}
