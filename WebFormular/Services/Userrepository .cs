using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace WebFormular.Data;

// ------------------------------------------------------------
// Modelle
// ------------------------------------------------------------
public sealed class User
{
    public Guid Id { get; init; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public List<string> Roles { get; } = new();
}

public sealed class Role
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class DuplicateUsernameException : Exception
{
    public DuplicateUsernameException(string username)
        : base($"Der Benutzername '{username}' ist bereits vergeben.") { }
}

// ------------------------------------------------------------
// Repository
// Registrierung (Program.cs):
//   builder.Services.AddScoped(_ =>
//       new UserRepository(builder.Configuration.GetConnectionString("WebFormular")!));
// ------------------------------------------------------------
public sealed class UserRepository
{
    private const string DefaultRole = "User";

    // Reihenfolge muss zu ReadUser() passen
    private const string UserColumns =
        "u.Id, u.Username, u.Email, u.IsActive, u.CreatedAt, u.LastLoginAt";

    private static readonly User HasherUser = new();

    private readonly string _connectionString;
    private readonly PasswordHasher<User> _hasher = new();

    public UserRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection String darf nicht leer sein.", nameof(connectionString));

        _connectionString = connectionString;
    }

    // ========================================================
    // Benutzer: lesen
    // ========================================================
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetSingleAsync("u.Id = @P", Guidp("@P", id), ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        GetSingleAsync("u.Username = @P", Text("@P", 50, username), ct);

    public async Task<List<User>> GetAllAsync(int skip = 0, int take = 100, CancellationToken ct = default)
    {
        var sql = $@"
SELECT {UserColumns}
FROM dbo.Users u
ORDER BY u.Username
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

SELECT ur.UserId, r.Name
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE ur.UserId IN (
    SELECT u.Id FROM dbo.Users u
    ORDER BY u.Username
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY)
ORDER BY r.Name;";

        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = Math.Max(0, skip);
        cmd.Parameters.Add("@Take", SqlDbType.Int).Value = Math.Clamp(take, 1, 1000);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var users = new List<User>();
        var byId = new Dictionary<Guid, User>();
        while (await reader.ReadAsync(ct))
        {
            var user = ReadUser(reader);
            users.Add(user);
            byId[user.Id] = user;
        }

        await reader.NextResultAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            if (byId.TryGetValue(reader.GetGuid(0), out var owner))
                owner.Roles.Add(reader.GetString(1));
        }

        return users;
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        var result = await ScalarAsync(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username) THEN 1 ELSE 0 END;",
            ct, Text("@Username", 50, username));
        return Convert.ToInt32(result) == 1;
    }

    // ========================================================
    // Benutzer: anlegen / ändern / löschen
    // ========================================================
    /// <summary>Legt einen Benutzer samt Rollen an (Standard: Rolle "User"). Das Passwort wird gehasht.</summary>
    public async Task<Guid> CreateUserAsync(
        string username,
        string password,
        string? email = null,
        IEnumerable<string>? roles = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var roleList = (roles ?? new[] { DefaultRole })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var hash = _hasher.HashPassword(HasherUser, password);

        await using var con = await OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);

        try
        {
            Guid id;
            await using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Users (Username, Email, PasswordHash)
OUTPUT INSERTED.Id
VALUES (@Username, @Email, @Hash);", con, tx))
            {
                cmd.Parameters.Add(Text("@Username", 50, username));
                cmd.Parameters.Add(Text("@Email", 256, email));
                cmd.Parameters.Add(Text("@Hash", 256, hash));
                id = (Guid)(await cmd.ExecuteScalarAsync(ct))!;
            }

            foreach (var role in roleList)
                await AddRoleInternalAsync(con, tx, id, role, ct);

            await tx.CommitAsync(ct);
            return id;
        }
        catch (SqlException ex) when (IsDuplicateKey(ex))
        {
            throw new DuplicateUsernameException(username);
        }
    }

    /// <summary>Aktualisiert Username, Email und IsActive. Passwort und Rollen bleiben unberührt.</summary>
    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            var rows = await ExecuteAsync(@"
UPDATE dbo.Users
SET Username = @Username, Email = @Email, IsActive = @IsActive
WHERE Id = @Id;",
                ct,
                Guidp("@Id", user.Id),
                Text("@Username", 50, user.Username),
                Text("@Email", 256, user.Email),
                Flag("@IsActive", user.IsActive));
            return rows > 0;
        }
        catch (SqlException ex) when (IsDuplicateKey(ex))
        {
            throw new DuplicateUsernameException(user.Username);
        }
    }

    public async Task<bool> SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default) =>
        await ExecuteAsync("UPDATE dbo.Users SET IsActive = @IsActive WHERE Id = @Id;",
            ct, Guidp("@Id", userId), Flag("@IsActive", isActive)) > 0;

    /// <summary>Löscht den Benutzer; UserRoles-Einträge werden per ON DELETE CASCADE entfernt.</summary>
    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default) =>
        await ExecuteAsync("DELETE FROM dbo.Users WHERE Id = @Id;", ct, Guidp("@Id", userId)) > 0;

    // ========================================================
    // Passwort / Login
    // ========================================================
    /// <summary>
    /// Prüft Benutzername + Passwort. Gibt bei Erfolg den Benutzer (inkl. Rollen) zurück, sonst null.
    /// Aktualisiert LastLoginAt und rehasht das Passwort bei Bedarf (z. B. nach Änderung der Hash-Parameter).
    /// </summary>
    public async Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return null;

        Guid id;
        string hash;
        bool isActive;

        await using (var con = await OpenAsync(ct))
        await using (var cmd = new SqlCommand(
            "SELECT Id, PasswordHash, IsActive FROM dbo.Users WHERE Username = @Username;", con))
        {
            cmd.Parameters.Add(Text("@Username", 50, username));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            id = reader.GetGuid(0);
            hash = reader.GetString(1);
            isActive = reader.GetBoolean(2);
        }

        var result = _hasher.VerifyHashedPassword(HasherUser, hash, password);
        if (result == PasswordVerificationResult.Failed || !isActive)
            return null;

        var newHash = result == PasswordVerificationResult.SuccessRehashNeeded
            ? _hasher.HashPassword(HasherUser, password)
            : null;

        await ExecuteAsync(@"
UPDATE dbo.Users
SET LastLoginAt = SYSUTCDATETIME(),
    PasswordHash = COALESCE(@NewHash, PasswordHash)
WHERE Id = @Id;",
            ct, Guidp("@Id", id), Text("@NewHash", 256, newHash));

        return await GetByIdAsync(id, ct);
    }

    /// <summary>Ändert das Passwort nach Prüfung des aktuellen Passworts.</summary>
    public async Task<bool> ChangePasswordAsync(
        Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        var current = await ScalarAsync(
            "SELECT PasswordHash FROM dbo.Users WHERE Id = @Id;", ct, Guidp("@Id", userId));
        if (current is not string currentHash)
            return false;

        var result = _hasher.VerifyHashedPassword(HasherUser, currentHash, currentPassword);
        if (result == PasswordVerificationResult.Failed)
            return false;

        return await ResetPasswordAsync(userId, newPassword, ct);
    }

    /// <summary>Setzt das Passwort ohne Prüfung des alten (z. B. für Admins).</summary>
    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        var hash = _hasher.HashPassword(HasherUser, newPassword);
        return await ExecuteAsync("UPDATE dbo.Users SET PasswordHash = @Hash WHERE Id = @Id;",
            ct, Guidp("@Id", userId), Text("@Hash", 256, hash)) > 0;
    }

    // ========================================================
    // Rollen
    // ========================================================
    public async Task<List<Role>> GetRolesAsync(CancellationToken ct = default)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT Id, Name FROM dbo.Roles ORDER BY Name;", con);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var roles = new List<Role>();
        while (await reader.ReadAsync(ct))
            roles.Add(new Role { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        return roles;
    }

    public async Task<int> CreateRoleAsync(string name, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        try
        {
            var id = await ScalarAsync(
                "INSERT INTO dbo.Roles (Name) OUTPUT INSERTED.Id VALUES (@Name);",
                ct, Text("@Name", 50, name));
            return Convert.ToInt32(id);
        }
        catch (SqlException ex) when (IsDuplicateKey(ex))
        {
            throw new InvalidOperationException($"Die Rolle '{name}' existiert bereits.");
        }
    }

    public async Task<bool> DeleteRoleAsync(string name, CancellationToken ct = default) =>
        await ExecuteAsync("DELETE FROM dbo.Roles WHERE Name = @Name;", ct, Text("@Name", 50, name)) > 0;

    public async Task<List<string>> GetRolesForUserAsync(Guid userId, CancellationToken ct = default)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(@"
SELECT r.Name
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE ur.UserId = @Id
ORDER BY r.Name;", con);
        cmd.Parameters.Add(Guidp("@Id", userId));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var roles = new List<string>();
        while (await reader.ReadAsync(ct))
            roles.Add(reader.GetString(0));
        return roles;
    }

    /// <summary>Liefert alle Benutzer einer Rolle (das Feld Roles der Benutzer bleibt dabei leer).</summary>
    public async Task<List<User>> GetUsersInRoleAsync(string roleName, CancellationToken ct = default)
    {
        var sql = $@"
SELECT {UserColumns}
FROM dbo.Users u
JOIN dbo.UserRoles ur ON ur.UserId = u.Id
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE r.Name = @Role
ORDER BY u.Username;";

        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(Text("@Role", 50, roleName));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var users = new List<User>();
        while (await reader.ReadAsync(ct))
            users.Add(ReadUser(reader));
        return users;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        var result = await ScalarAsync(@"
SELECT CASE WHEN EXISTS (
    SELECT 1
    FROM dbo.UserRoles ur
    JOIN dbo.Roles r ON r.Id = ur.RoleId
    WHERE ur.UserId = @UserId AND r.Name = @Role) THEN 1 ELSE 0 END;",
            ct, Guidp("@UserId", userId), Text("@Role", 50, roleName));
        return Convert.ToInt32(result) == 1;
    }

    /// <summary>Weist eine Rolle zu. Gibt false zurück, wenn der Benutzer sie bereits hat.</summary>
    public async Task<bool> AddUserToRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        await using var con = await OpenAsync(ct);
        return await AddRoleInternalAsync(con, null, userId, roleName, ct);
    }

    public async Task<bool> RemoveUserFromRoleAsync(Guid userId, string roleName, CancellationToken ct = default) =>
        await ExecuteAsync(@"
DELETE ur
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE ur.UserId = @UserId AND r.Name = @Role;",
            ct, Guidp("@UserId", userId), Text("@Role", 50, roleName)) > 0;

    /// <summary>Ersetzt alle Rollen des Benutzers durch die übergebene Liste (in einer Transaktion).</summary>
    public async Task SetRolesAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var roleList = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        await using var con = await OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);

        await using (var cmd = new SqlCommand("DELETE FROM dbo.UserRoles WHERE UserId = @UserId;", con, tx))
        {
            cmd.Parameters.Add(Guidp("@UserId", userId));
            await cmd.ExecuteNonQueryAsync(ct);
        }

        foreach (var role in roleList)
            await AddRoleInternalAsync(con, tx, userId, role, ct);

        await tx.CommitAsync(ct);
    }

    // ========================================================
    // Interne Helfer
    // ========================================================
    private async Task<SqlConnection> OpenAsync(CancellationToken ct)
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

    private async Task<User?> GetSingleAsync(string where, SqlParameter parameter, CancellationToken ct)
    {
        // "where" stammt ausschließlich aus festen Strings dieser Klasse, Werte laufen über Parameter
        var sql = $@"
SELECT {UserColumns}
FROM dbo.Users u
WHERE {where};

SELECT r.Name
FROM dbo.Users u
JOIN dbo.UserRoles ur ON ur.UserId = u.Id
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE {where}
ORDER BY r.Name;";

        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(parameter);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var user = ReadUser(reader);
        await reader.NextResultAsync(ct);
        while (await reader.ReadAsync(ct))
            user.Roles.Add(reader.GetString(0));
        return user;
    }

    private static async Task<bool> AddRoleInternalAsync(
        SqlConnection con, SqlTransaction? tx, Guid userId, string roleName, CancellationToken ct)
    {
        const string sql = @"
DECLARE @RoleId INT = (SELECT Id FROM dbo.Roles WHERE Name = @Role);
IF @RoleId IS NULL BEGIN SELECT -1; RETURN; END
IF EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
BEGIN SELECT 0; RETURN; END
INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
SELECT 1;";

        await using var cmd = new SqlCommand(sql, con, tx);
        cmd.Parameters.Add(Guidp("@UserId", userId));
        cmd.Parameters.Add(Text("@Role", 50, roleName));

        var result = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        if (result == -1)
            throw new InvalidOperationException($"Die Rolle '{roleName}' existiert nicht.");
        return result == 1;
    }

    private async Task<int> ExecuteAsync(string sql, CancellationToken ct, params SqlParameter[] parameters)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<object?> ScalarAsync(string sql, CancellationToken ct, params SqlParameter[] parameters)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteScalarAsync(ct);
    }

    private static User ReadUser(SqlDataReader r)
    {
        return new User
        {
            Id = r.GetGuid(0),
            Username = r.GetString(1),
            Email = r.IsDBNull(2) ? null : r.GetString(2),
            IsActive = r.GetBoolean(3),
            CreatedAt = r.GetDateTime(4),
            LastLoginAt = r.IsDBNull(5) ? null : r.GetDateTime(5)
        };
    }

    // 2601 / 2627 = Unique-Constraint- bzw. Unique-Index-Verletzung
    private static bool IsDuplicateKey(SqlException ex) => ex.Number is 2601 or 2627;

    private static SqlParameter Guidp(string name, Guid value) =>
        new(name, SqlDbType.UniqueIdentifier) { Value = value };

    private static SqlParameter Text(string name, int size, string? value) =>
        new(name, SqlDbType.NVarChar, size) { Value = (object?)value ?? DBNull.Value };

    private static SqlParameter Flag(string name, bool value) =>
        new(name, SqlDbType.Bit) { Value = value };
}