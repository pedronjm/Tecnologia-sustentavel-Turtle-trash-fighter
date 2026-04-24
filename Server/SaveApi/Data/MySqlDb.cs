using MySqlConnector;

namespace SaveApi.Data;

public sealed class MySqlDb
{
    private readonly string _connectionString;
    private readonly MySqlConnectionStringBuilder _connectionStringBuilder;

    public MySqlDb(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' nao configurada.");

        _connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
    }

    public MySqlConnection CreateConnection() => new(_connectionString);

    public object GetConnectionInfo() =>
        new
        {
            Server = _connectionStringBuilder.Server,
            Port = _connectionStringBuilder.Port,
            Database = _connectionStringBuilder.Database,
            User = _connectionStringBuilder.UserID,
            SslMode = _connectionStringBuilder.SslMode.ToString(),
        };
}
