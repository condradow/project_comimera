using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebFormular.Classes.Elements;

namespace WebFormular.Data;

/// <summary>
/// Optional: legt den in der Datenbank gespeicherten Typnamen eines Elements fest.
/// Ohne Attribut wird der Klassenname verwendet. Mit Attribut kann die Klasse später
/// umbenannt oder verschoben werden, ohne dass gespeicherte Daten unlesbar werden.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ElementTypeNameAttribute : Attribute
{
    public string Name { get; }
    public ElementTypeNameAttribute(string name) => Name = name;
}

/// <summary>Ein aus der Datenbank gelesenes, typisiertes Element samt Zeilen-Id und Audit-Feldern.</summary>
public sealed class StoredElement<T> where T : ElementBase
{
    public Guid Id { get; init; }
    public T Element { get; init; } = null!;
    public DateTime? CreatedOn { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public string? ModifiedBy { get; init; }
}

/// <summary>
/// Serialisiert beliebige ElementBase-Ableitungen nach JSON und zurück.
/// Der Typ steht als Eigenschaft "ElementType" im JSON. Neue Elementklassen werden
/// automatisch gefunden (öffentlich, nicht abstrakt, parameterloser Konstruktor,
/// gleiche Assembly wie ElementBase) – eine Registrierung ist nicht nötig.
/// Document und DocumentId werden nicht gespeichert (stehen in der Tabelle).
/// </summary>
public static class ElementJson
{
    public const string TypeProperty = "ElementType";

    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.Ordinal)
    {
        nameof(ElementBase.Document),
        nameof(ElementBase.DocumentId)
    };

    private static readonly JsonSerializerOptions Options = CreateOptions();
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> Types = new(DiscoverTypes);

    // ---------- Schreiben ----------
    public static string Serialize(ElementBase element)
    {
        ArgumentNullException.ThrowIfNull(element);

        var type = element.GetType();
        var typeName = GetTypeName(type);
        if (!Types.Value.TryGetValue(typeName, out var registered) || registered != type)
            throw new InvalidOperationException(
                $"Der Elementtyp {type.FullName} wird nicht unterstützt (abstrakt, nicht öffentlich oder ohne parameterlosen Konstruktor).");

        var node = (JsonObject)JsonSerializer.SerializeToNode(element, type, Options)!;

        // Typ als erste Eigenschaft, damit er auch in gekürzter Anzeige sichtbar ist
        var result = new JsonObject { [TypeProperty] = typeName };
        foreach (var (key, value) in node.ToArray())
        {
            node.Remove(key);
            result[key] = value;
        }

        return result.ToJsonString(Options);
    }

    // ---------- Lesen ----------
    /// <summary>Liest ein Element. Wirft eine JsonException bei ungültigem JSON, fehlender oder unbekannter Typangabe.</summary>
    public static ElementBase Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var node = JsonNode.Parse(json) as JsonObject
            ?? throw new JsonException("JSONData ist kein JSON-Objekt.");

        if (node[TypeProperty] is not JsonValue typeValue
            || !typeValue.TryGetValue<string>(out var typeName)
            || string.IsNullOrEmpty(typeName))
            throw new JsonException($"Die Typangabe '{TypeProperty}' fehlt.");

        if (!Types.Value.TryGetValue(typeName, out var type))
            throw new JsonException($"Unbekannter Elementtyp '{typeName}'.");

        node.Remove(TypeProperty);

        var element = node.Deserialize(type, Options)
            ?? throw new JsonException($"Element vom Typ '{typeName}' konnte nicht gelesen werden.");
        return (ElementBase)element;
    }

    /// <summary>Wie Deserialize, gibt aber false statt einer Exception zurück.</summary>
    public static bool TryDeserialize(string? json, out ElementBase? element)
    {
        element = null;
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            element = Deserialize(json);
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or NotSupportedException)
        {
            return false;
        }
    }

    public static string GetTypeName(Type type) =>
        type.GetCustomAttribute<ElementTypeNameAttribute>(inherit: false)?.Name ?? type.Name;

    // ---------- Intern ----------
    private static JsonSerializerOptions CreateOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object || !typeof(ElementBase).IsAssignableFrom(typeInfo.Type))
                return;

            for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
            {
                if (IgnoredProperties.Contains(typeInfo.Properties[i].Name))
                    typeInfo.Properties.RemoveAt(i);
            }
        });

        return new JsonSerializerOptions
        {
            TypeInfoResolver = resolver,
            // Enums als Text speichern (lesbar und unabhängig von der Reihenfolge der Werte)
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private static Dictionary<string, Type> DiscoverTypes()
    {
        var map = new Dictionary<string, Type>(StringComparer.Ordinal);

        var candidates = typeof(ElementBase).Assembly.GetTypes().Where(t =>
            t.IsClass && !t.IsAbstract && typeof(ElementBase).IsAssignableFrom(t)
            && t.GetConstructor(Type.EmptyTypes) is not null);

        foreach (var type in candidates)
        {
            var name = GetTypeName(type);
            if (!map.TryAdd(name, type))
                throw new InvalidOperationException(
                    $"Der Elementtyp-Name '{name}' ist doppelt vergeben ({map[name].FullName} und {type.FullName}). " +
                    "Vergib mit [ElementTypeName(\"...\")] einen eindeutigen Namen.");
        }

        return map;
    }
}
