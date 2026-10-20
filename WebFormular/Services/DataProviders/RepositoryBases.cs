using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using WebFormular.Classes.Elements;

namespace WebFormular.Data;

// ============================================================
// Basis für dbo.Documents / dbo.Templates
// Tabellen- und Spaltennamen kommen ausschließlich aus festen Strings
// der abgeleiteten Klassen, alle Werte laufen über Parameter.
// ============================================================
public abstract class FormRepositoryBase<T> : SqlRepositoryBase where T : FormEntity, new()
{
    private const string Columns =
        "Id, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, InUntis, ApprovedBySubstitu, ApprovedByPrincipal, Title, Description";

    private const string SearchWhere =
        @"(@Pattern IS NULL OR Title LIKE @Pattern ESCAPE '\' OR Description LIKE @Pattern ESCAPE '\')";

    private readonly string _table;
    private readonly string _childTable;
    private readonly string _childParentColumn;

    protected FormRepositoryBase(string connectionString, string table, string childTable, string childParentColumn)
        : base(connectionString)
    {
        _table = table;
        _childTable = childTable;
        _childParentColumn = childParentColumn;
    }

    // ---------- Lesen ----------
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sql = $"SELECT {Columns} FROM dbo.{_table} WHERE Id = @Id;";

        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(Guidp("@Id", id));
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    /// <summary>Liste mit optionaler Suche in Title/Description, neueste Änderungen zuerst.</summary>
    public async Task<List<T>> GetAllAsync(
        string? search = null, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        var sql = $@"
SELECT {Columns}
FROM dbo.{_table}
WHERE {SearchWhere}
ORDER BY ModifiedOn DESC, Title, Id
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(LongText("@Pattern", BuildPattern(search)));
        cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = Math.Max(0, skip);
        cmd.Parameters.Add("@Take", SqlDbType.Int).Value = Math.Clamp(take, 1, 1000);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<T>();
        while (await reader.ReadAsync(ct))
            list.Add(Read(reader));
        return list;
    }

    public async Task<int> CountAsync(string? search = null, CancellationToken ct = default)
    {
        var result = await ScalarAsync(
            $"SELECT COUNT(*) FROM dbo.{_table} WHERE {SearchWhere};",
            ct, LongText("@Pattern", BuildPattern(search)));
        return Convert.ToInt32(result);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        var result = await ScalarAsync(
            $"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.{_table} WHERE Id = @Id) THEN 1 ELSE 0 END;",
            ct, Guidp("@Id", id));
        return Convert.ToInt32(result) == 1;
    }

    // ---------- Anlegen / Ändern ----------
    /// <summary>Legt einen Eintrag an. Id (falls leer) sowie Created*/Modified* werden gesetzt.</summary>
    public async Task<Guid> CreateAsync(T entity, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        var now = DateTime.UtcNow;
        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();
        entity.CreatedOn = entity.ModifiedOn = now;
        entity.CreatedBy = entity.ModifiedBy = user;

        await ExecuteAsync($@"
INSERT INTO dbo.{_table} ({Columns})
VALUES (@Id, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy, @InUntis, @ApprovedBySubstitu, @ApprovedByPrincipal, @Title, @Description);",
            ct,
            Guidp("@Id", entity.Id),
            Date("@CreatedOn", entity.CreatedOn),
            Text("@CreatedBy", 100, entity.CreatedBy),
            Date("@ModifiedOn", entity.ModifiedOn),
            Text("@ModifiedBy", 100, entity.ModifiedBy),
            Flag("@InUntis", entity.InUntis),
            Flag("@ApprovedBySubstitu", entity.ApprovedBySubstitu),
            Flag("@ApprovedByPrincipal", entity.ApprovedByPrincipal),
            Text("@Title", 200, entity.Title),
            Text("@Description", 200, entity.Description));

        return entity.Id;
    }

    /// <summary>
    /// Aktualisiert Title und Description. Die Status-Flags (InUntis, Genehmigungen)
    /// werden bewusst nur über die Set…Async-Methoden geändert.
    /// </summary>
    public async Task<bool> UpdateAsync(T entity, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = user;

        var rows = await ExecuteAsync($@"
UPDATE dbo.{_table}
SET Title = @Title, Description = @Description, ModifiedOn = @ModifiedOn, ModifiedBy = @ModifiedBy
WHERE Id = @Id;",
            ct,
            Guidp("@Id", entity.Id),
            Text("@Title", 200, entity.Title),
            Text("@Description", 200, entity.Description),
            Date("@ModifiedOn", entity.ModifiedOn),
            Text("@ModifiedBy", 100, entity.ModifiedBy));

        return rows > 0;
    }

    public Task<bool> SetInUntisAsync(Guid id, bool value, string user, CancellationToken ct = default) =>
        SetBitAsync("InUntis", id, value, user, ct);

    public Task<bool> SetApprovedBySubstituAsync(Guid id, bool value, string user, CancellationToken ct = default) =>
        SetBitAsync("ApprovedBySubstitu", id, value, user, ct);

    public Task<bool> SetApprovedByPrincipalAsync(Guid id, bool value, string user, CancellationToken ct = default) =>
        SetBitAsync("ApprovedByPrincipal", id, value, user, ct);

    // ---------- Löschen ----------
    /// <summary>Löscht den Eintrag samt zugehöriger Elemente (die FKs haben kein ON DELETE CASCADE) atomar.</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var result = await ScalarAsync($@"
SET XACT_ABORT ON;
BEGIN TRAN;
DELETE FROM dbo.{_childTable} WHERE {_childParentColumn} = @Id;
DELETE FROM dbo.{_table} WHERE Id = @Id;
DECLARE @Deleted INT = @@ROWCOUNT;
COMMIT;
SELECT @Deleted;",
            ct, Guidp("@Id", id));

        return Convert.ToInt32(result) > 0;
    }

    // ---------- Intern ----------
    private async Task<bool> SetBitAsync(string column, Guid id, bool value, string user, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        var rows = await ExecuteAsync($@"
UPDATE dbo.{_table}
SET {column} = @Value, ModifiedOn = @ModifiedOn, ModifiedBy = @ModifiedBy
WHERE Id = @Id;",
            ct,
            Guidp("@Id", id),
            Flag("@Value", value),
            Date("@ModifiedOn", DateTime.UtcNow),
            Text("@ModifiedBy", 100, user));

        return rows > 0;
    }

    private static string? BuildPattern(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return null;

        var escaped = search.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_")
            .Replace("[", "\\[");
        return $"%{escaped}%";
    }

    // Reihenfolge muss zu Columns passen
    private static T Read(SqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        CreatedOn = ReadDate(r, 1),
        CreatedBy = ReadString(r, 2),
        ModifiedOn = ReadDate(r, 3),
        ModifiedBy = ReadString(r, 4),
        InUntis = ReadBool(r, 5),
        ApprovedBySubstitu = ReadBool(r, 6),
        ApprovedByPrincipal = ReadBool(r, 7),
        Title = ReadString(r, 8),
        Description = ReadString(r, 9)
    };
}

// ============================================================
// Basis für dbo.Elements / dbo.TemplateElements
// ============================================================
public abstract class ElementRepositoryBase<T> : SqlRepositoryBase where T : ElementEntity, new()
{
    private readonly string _table;
    private readonly string _parentColumn;

    protected ElementRepositoryBase(string connectionString, string table, string parentColumn)
        : base(connectionString)
    {
        _table = table;
        _parentColumn = parentColumn;
    }

    private string Columns => $"Id, {_parentColumn}, JSONData, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy";

    // ---------- Lesen ----------
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand($"SELECT {Columns} FROM dbo.{_table} WHERE Id = @Id;", con);
        cmd.Parameters.Add(Guidp("@Id", id));
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    /// <summary>
    /// Alle Elemente eines Dokuments bzw. Templates. Die Tabelle hat keine Sortierspalte,
    /// daher wird nach CreatedOn sortiert (siehe Hinweis zu SortOrder).
    /// </summary>
    public async Task<List<T>> GetByParentAsync(Guid parentId, CancellationToken ct = default)
    {
        var sql = $"SELECT {Columns} FROM dbo.{_table} WHERE {_parentColumn} = @ParentId ORDER BY CreatedOn, Id;";

        await using var con = await OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(Guidp("@ParentId", parentId));
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<T>();
        while (await reader.ReadAsync(ct))
            list.Add(Read(reader));
        return list;
    }

    public async Task<int> CountByParentAsync(Guid parentId, CancellationToken ct = default)
    {
        var result = await ScalarAsync(
            $"SELECT COUNT(*) FROM dbo.{_table} WHERE {_parentColumn} = @ParentId;",
            ct, Guidp("@ParentId", parentId));
        return Convert.ToInt32(result);
    }

    // ---------- Anlegen / Ändern ----------
    public async Task<Guid> CreateAsync(Guid parentId, T element, string user, CancellationToken ct = default)
    {
        await CreateManyAsync(parentId, new[] { element }, user, ct);
        return element.Id;
    }

    /// <summary>Legt mehrere Elemente in einer Transaktion an (alles oder nichts).</summary>
    public async Task CreateManyAsync(Guid parentId, IEnumerable<T> elements, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        var list = elements.ToList();
        if (list.Count == 0)
            return;

        var now = DateTime.UtcNow;

        await using var con = await OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        await using var cmd = new SqlCommand($@"
INSERT INTO dbo.{_table} ({Columns})
VALUES (@Id, @ParentId, @Json, @On, @By, @On, @By);", con, tx);

        var pId = cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        var pParent = cmd.Parameters.Add("@ParentId", SqlDbType.UniqueIdentifier);
        var pJson = cmd.Parameters.Add("@Json", SqlDbType.NVarChar, -1);
        var pOn = cmd.Parameters.Add("@On", SqlDbType.DateTime);
        var pBy = cmd.Parameters.Add("@By", SqlDbType.NVarChar, 100);

        // 10 ms Versatz pro Element bei CreatedOn, damit die Reihenfolge erhalten bleibt
        // (die Tabelle hat keine Sortierspalte; datetime hat nur ca. 3 ms Auflösung).
        var index = 0;
        foreach (var element in list)
        {
            var stamp = now.AddMilliseconds(index++ * 10);

            if (element.Id == Guid.Empty)
                element.Id = Guid.NewGuid();
            element.ParentId = parentId;
            element.CreatedOn = element.ModifiedOn = stamp;
            element.CreatedBy = element.ModifiedBy = user;

            pId.Value = element.Id;
            pParent.Value = parentId;
            pJson.Value = (object?)element.JsonData ?? DBNull.Value;
            pOn.Value = stamp;
            pBy.Value = user;

            await cmd.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    /// <summary>Aktualisiert JSONData des Elements.</summary>
    public async Task<bool> UpdateAsync(T element, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        element.ModifiedOn = DateTime.UtcNow;
        element.ModifiedBy = user;

        var rows = await ExecuteAsync($@"
UPDATE dbo.{_table}
SET JSONData = @Json, ModifiedOn = @ModifiedOn, ModifiedBy = @ModifiedBy
WHERE Id = @Id;",
            ct,
            Guidp("@Id", element.Id),
            LongText("@Json", element.JsonData),
            Date("@ModifiedOn", element.ModifiedOn),
            Text("@ModifiedBy", 100, element.ModifiedBy));

        return rows > 0;
    }

    // ---------- Löschen ----------
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        await ExecuteAsync($"DELETE FROM dbo.{_table} WHERE Id = @Id;", ct, Guidp("@Id", id)) > 0;

    /// <summary>Löscht alle Elemente eines Dokuments bzw. Templates und gibt die Anzahl zurück.</summary>
    public Task<int> DeleteByParentAsync(Guid parentId, CancellationToken ct = default) =>
        ExecuteAsync($"DELETE FROM dbo.{_table} WHERE {_parentColumn} = @ParentId;", ct, Guidp("@ParentId", parentId));

    // ---------- Typisierte Elemente (ElementBase und alle Ableitungen) ----------
    /// <summary>Legt ein Element (z. B. ApplicantLessonChangeRequest) an; der Typ wird im JSON mitgespeichert.</summary>
    public async Task<Guid> AddElementAsync(Guid parentId, ElementBase element, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.DocumentId = parentId;
        var row = new T { JsonData = ElementJson.Serialize(element) };
        return await CreateAsync(parentId, row, user, ct);
    }

    /// <summary>Legt mehrere Elemente in einer Transaktion an; die Reihenfolge der Liste bleibt erhalten.</summary>
    public async Task AddElementsAsync(Guid parentId, IEnumerable<ElementBase> elements, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(elements);

        var rows = new List<T>();
        foreach (var element in elements)
        {
            element.DocumentId = parentId;
            rows.Add(new T { JsonData = ElementJson.Serialize(element) });
        }

        await CreateManyAsync(parentId, rows, user, ct);
    }

    /// <summary>Überschreibt die Daten des Elements mit der angegebenen Zeilen-Id.</summary>
    public async Task<bool> UpdateElementAsync(Guid id, ElementBase element, string user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(element);

        var row = new T { Id = id, JsonData = ElementJson.Serialize(element) };
        return await UpdateAsync(row, user, ct);
    }

    /// <summary>Liest ein einzelnes Element. Wirft eine InvalidOperationException, wenn das JSON nicht lesbar ist.</summary>
    public async Task<StoredElement<ElementBase>?> GetElementAsync(Guid id, CancellationToken ct = default)
    {
        var row = await GetByIdAsync(id, ct);
        return row is null ? null : ToStored(row, ReadTyped(row));
    }

    /// <summary>Alle Elemente eines Dokuments bzw. Templates als typisierte Objekte. Nicht lesbare Zeilen lösen eine Exception aus.</summary>
    public async Task<List<StoredElement<ElementBase>>> GetElementsAsync(Guid parentId, CancellationToken ct = default)
    {
        var rows = await GetByParentAsync(parentId, ct);
        return rows.Select(row => ToStored(row, ReadTyped(row))).ToList();
    }

    /// <summary>Nur Elemente eines bestimmten Typs (inkl. Ableitungen). Nicht lesbare oder fremde Zeilen werden übersprungen.</summary>
    public async Task<List<StoredElement<TElement>>> GetElementsOfTypeAsync<TElement>(Guid parentId, CancellationToken ct = default)
        where TElement : ElementBase
    {
        var rows = await GetByParentAsync(parentId, ct);
        var result = new List<StoredElement<TElement>>();

        foreach (var row in rows)
        {
            if (ElementJson.TryDeserialize(row.JsonData, out var element) && element is TElement typed)
                result.Add(ToStored(row, typed));
        }

        return result;
    }

    private static ElementBase ReadTyped(T row)
    {
        if (string.IsNullOrWhiteSpace(row.JsonData))
            throw new InvalidOperationException($"Element {row.Id} hat kein JSONData.");

        try
        {
            return ElementJson.Deserialize(row.JsonData);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Element {row.Id} kann nicht gelesen werden: {ex.Message}", ex);
        }
    }

    // DocumentId des Elements wird aus der Tabelle gesetzt (bei TemplateElements ist das die TemplateId)
    private static StoredElement<TElement> ToStored<TElement>(T row, TElement element) where TElement : ElementBase
    {
        element.DocumentId = row.ParentId ?? Guid.Empty;
        return new StoredElement<TElement>
        {
            Id = row.Id,
            Element = element,
            CreatedOn = row.CreatedOn,
            CreatedBy = row.CreatedBy,
            ModifiedOn = row.ModifiedOn,
            ModifiedBy = row.ModifiedBy
        };
    }

    // Reihenfolge muss zu Columns passen
    private static T Read(SqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        ParentId = ReadGuid(r, 1),
        JsonData = ReadString(r, 2),
        CreatedOn = ReadDate(r, 3),
        CreatedBy = ReadString(r, 4),
        ModifiedOn = ReadDate(r, 5),
        ModifiedBy = ReadString(r, 6)
    };
}
