using Microsoft.Data.SqlClient;
using System.Data;

public class SqlConnectionService
{
    private readonly string _connectionString;

    public SqlConnectionService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException(
                "Der Connection String darf nicht leer sein.",
                nameof(connectionString));

        _connectionString = connectionString;
    }

    /// <summary>
    /// Erstellt eine neue SQL-Verbindung.
    /// </summary>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Öffnet eine SQL-Verbindung und testet die Verbindung.
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            return connection.State == ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Führt ein SQL-Statement aus und gibt die Anzahl
    /// der betroffenen Zeilen zurück.
    /// </summary>
    public async Task<int> ExecuteAsync(
        string sql,
        params SqlParameter[] parameters)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Führt eine SQL-Abfrage aus und gibt das Ergebnis
    /// als SqlDataReader zurück.
    /// </summary>
    public async Task<SqlDataReader> QueryAsync(
        string sql,
        params SqlParameter[] parameters)
    {
        var connection = CreateConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(sql, connection);

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        return await command.ExecuteReaderAsync(
            CommandBehavior.CloseConnection);
    }
}