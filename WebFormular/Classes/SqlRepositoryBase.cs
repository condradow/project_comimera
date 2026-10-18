using System.Data;
using Microsoft.Data.SqlClient;

namespace WebFormular.Data;

/// <summary>
/// Basisklasse für alle Repositories: Connection String als Parameter, 
/// Hilfsmethoden für Ausführung, Parameter und das Lesen von NULL-Werten.
/// </summary>
public abstract class SqlRepositoryBase
{
    private readonly string _connectionString;

    protected SqlRepositoryBase(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection String darf nicht leer sein.", nameof(connectionString));

        _connectionString = connectionString;
    }

    // ---------- Verbindung / Ausführung ----------
    protected async Task<SqlConnection> OpenAsync(CancellationToken ct)
    {
        var con = new SqlConnection(_connectionString);
        try
        {
            await con.OpenAsync(ct);
            return con;
        }
        catch
        {
            await con.DisposeAsync();
            throw;
        }
    }

    protected async Task<int> ExecuteAsync(string sql, CancellationToken ct, params SqlParameter[] parameters)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    protected async Task<object?> ScalarAsync(string sql, CancellationToken ct, params SqlParameter[] parameters)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteScalarAsync(ct);
    }

    // ---------- Parameter ----------
    protected static SqlParameter Guidp(string name, Guid value) =>
        new(name, SqlDbType.UniqueIdentifier) { Value = value };

    protected static SqlParameter Text(string name, int size, string? value) =>
        new(name, SqlDbType.NVarChar, size) { Value = (object?)value ?? DBNull.Value };

    /// <summary>NVARCHAR(MAX), z. B. für JSONData.</summary>
    protected static SqlParameter LongText(string name, string? value) =>
        new(name, SqlDbType.NVarChar, -1) { Value = (object?)value ?? DBNull.Value };

    protected static SqlParameter Flag(string name, bool? value) =>
        new(name, SqlDbType.Bit) { Value = (object?)value ?? DBNull.Value };

    protected static SqlParameter Date(string name, DateTime? value) =>
        new(name, SqlDbType.DateTime) { Value = (object?)value ?? DBNull.Value };

    // ---------- Lesen (NULL-sicher) ----------
    protected static string? ReadString(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);
    protected static DateTime? ReadDate(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetDateTime(i);
    protected static bool? ReadBool(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetBoolean(i);
    protected static Guid? ReadGuid(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetGuid(i);
}
