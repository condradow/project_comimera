using System.Text.Json;

namespace WebFormular.Data;

/// <summary>Gemeinsame Felder von dbo.Documents und dbo.Templates (identische Spalten).</summary>
public abstract class FormEntity
{
    public Guid Id { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public bool? InUntis { get; set; }
    public bool? ApprovedBySubstitu { get; set; }
    public bool? ApprovedByPrincipal { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public sealed class Document : FormEntity
{
    public List<Element> Elements { get; } = new();
}

public sealed class Template : FormEntity
{
    public List<TemplateElement> Elements { get; } = new();
}

/// <summary>Gemeinsame Felder von dbo.Elements und dbo.TemplateElements.</summary>
public abstract class ElementEntity
{
    public Guid Id { get; set; }

    /// <summary>DocumentId bzw. TemplateId.</summary>
    public Guid? ParentId { get; set; }

    public string? JsonData { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    /// <summary>Deserialisiert JSONData in ein Objekt.</summary>
    public T? GetData<T>(JsonSerializerOptions? options = null) =>
        string.IsNullOrWhiteSpace(JsonData) ? default : JsonSerializer.Deserialize<T>(JsonData, options);

    /// <summary>Serialisiert ein Objekt nach JSONData.</summary>
    public void SetData<T>(T value, JsonSerializerOptions? options = null) =>
        JsonData = JsonSerializer.Serialize(value, options);
}

public sealed class Element : ElementEntity
{
    public Guid? DocumentId
    {
        get => ParentId;
        set => ParentId = value;
    }
}

public sealed class TemplateElement : ElementEntity
{
    public Guid? TemplateId
    {
        get => ParentId;
        set => ParentId = value;
    }
}
