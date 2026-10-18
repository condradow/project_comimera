namespace WebFormular.Data;

// ------------------------------------------------------------
// Registrierung (Program.cs):
//   var cs = builder.Configuration.GetConnectionString("WebFormular")!;
//   builder.Services.AddScoped(_ => new UserRepository(cs));
//   builder.Services.AddScoped(_ => new DocumentRepository(cs));
//   builder.Services.AddScoped(_ => new TemplateRepository(cs));
// ------------------------------------------------------------

public sealed class ElementRepository : ElementRepositoryBase<Element>
{
    public ElementRepository(string connectionString)
        : base(connectionString, "Elements", "DocumentId") { }
}

public sealed class TemplateElementRepository : ElementRepositoryBase<TemplateElement>
{
    public TemplateElementRepository(string connectionString)
        : base(connectionString, "TemplateElements", "TemplateId") { }
}

public sealed class DocumentRepository : FormRepositoryBase<Document>
{
    /// <summary>Zugriff auf die Elemente der Dokumente (dbo.Elements).</summary>
    public ElementRepository Elements { get; }

    public DocumentRepository(string connectionString)
        : base(connectionString, "Documents", "Elements", "DocumentId")
    {
        Elements = new ElementRepository(connectionString);
    }

    /// <summary>Lädt das Dokument inklusive seiner Elemente.</summary>
    public async Task<Document?> GetWithElementsAsync(Guid id, CancellationToken ct = default)
    {
        var document = await GetByIdAsync(id, ct);
        if (document is null)
            return null;

        document.Elements.AddRange(await Elements.GetByParentAsync(id, ct));
        return document;
    }

    /// <summary>
    /// Erzeugt ein neues Dokument als Kopie eines Templates (Kopfdaten + alle TemplateElements)
    /// in einer Transaktion. Status-Flags starten mit false. Gibt die neue Id zurück oder
    /// null, wenn das Template nicht existiert.
    /// </summary>
    public async Task<Guid?> CreateFromTemplateAsync(Guid templateId, string user, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user);

        var newId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // CreatedOn der Template-Elemente wird übernommen, damit die Reihenfolge erhalten bleibt
        // (die Elements-Tabelle hat keine eigene Sortierspalte).
        var result = await ScalarAsync(@"
SET XACT_ABORT ON;
BEGIN TRAN;

INSERT INTO dbo.Documents
    (Id, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, InUntis, ApprovedBySubstitu, ApprovedByPrincipal, Title, Description)
SELECT @NewId, @Now, @User, @Now, @User, 0, 0, 0, Title, Description
FROM dbo.Templates
WHERE Id = @TemplateId;

IF @@ROWCOUNT = 0
BEGIN
    ROLLBACK;
    SELECT 0;
    RETURN;
END

INSERT INTO dbo.Elements (Id, DocumentId, JSONData, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy)
SELECT NEWID(), @NewId, JSONData, ISNULL(CreatedOn, @Now), @User, @Now, @User
FROM dbo.TemplateElements
WHERE TemplateId = @TemplateId;

COMMIT;
SELECT 1;",
            ct,
            Guidp("@NewId", newId),
            Guidp("@TemplateId", templateId),
            Date("@Now", now),
            Text("@User", 100, user));

        return Convert.ToInt32(result) == 1 ? newId : null;
    }
}

public sealed class TemplateRepository : FormRepositoryBase<Template>
{
    /// <summary>Zugriff auf die Elemente der Templates (dbo.TemplateElements).</summary>
    public TemplateElementRepository Elements { get; }

    public TemplateRepository(string connectionString)
        : base(connectionString, "Templates", "TemplateElements", "TemplateId")
    {
        Elements = new TemplateElementRepository(connectionString);
    }

    /// <summary>Lädt das Template inklusive seiner Elemente.</summary>
    public async Task<Template?> GetWithElementsAsync(Guid id, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct);
        if (template is null)
            return null;

        template.Elements.AddRange(await Elements.GetByParentAsync(id, ct));
        return template;
    }
}
