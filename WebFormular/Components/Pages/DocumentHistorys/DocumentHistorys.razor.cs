using MudBlazor;
using WebFormular.Classes;

namespace WebFormular.Components.Pages.DocumentHistorys
{
    public partial class DocumentHistorys
    {
        private List<DocumentHistory> HistoryEntries = new();

        protected override void OnInitialized()
        {
            HistoryEntries = GetExampleHistory()
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
        private string SelectedHistory = "Alle";

        private Variant GetVariant(string value)
            => SelectedHistory == value
                ? Variant.Filled
                : Variant.Outlined;
        private static Color GetActivityColor(string activity)
        {
            return activity switch
            {
                "Unterschrieben" => Color.Success,
                "Abgelehnt" => Color.Error,
                "Erstellt" => Color.Info,
                "Freigegeben" => Color.Primary,
                _ => Color.Default
            };
        }

        private static List<DocumentHistory> GetExampleHistory()
        {
            return new List<DocumentHistory>
        {
            new()
            {
                UserName = "max.mustermann",
                DocumentName = "Arbeitsvertrag_Mueller.pdf",
                Activity = "Unterschrieben",
                Timestamp = new DateTime(2026, 10, 23, 08, 42, 15)
            },
            new()
            {
                UserName = "anna.schmidt",
                DocumentName = "Angebot_2026_104.pdf",
                Activity = "Abgelehnt",
                Timestamp = new DateTime(2026, 10, 23, 08, 15, 30)
            },
            new()
            {
                UserName = "peter.weber",
                DocumentName = "Geheimhaltungsvereinbarung.pdf",
                Activity = "Freigegeben",
                Timestamp = new DateTime(2026, 10, 22, 16, 54, 02)
            },
            new()
            {
                UserName = "max.mustermann",
                DocumentName = "Rechnung_2026_081.pdf",
                Activity = "Erstellt",
                Timestamp = new DateTime(2026, 10, 22, 14, 20, 45)
            },
            new()
            {
                UserName = "sarah.fischer",
                DocumentName = "Datenschutzvereinbarung.pdf",
                Activity = "Unterschrieben",
                Timestamp = new DateTime(2026, 10, 22, 11, 07, 18)
            },
            new()
            {
                UserName = "anna.schmidt",
                DocumentName = "Angebot_2026_104.pdf",
                Activity = "Erstellt",
                Timestamp = new DateTime(2026, 10, 21, 09, 30, 00)
            },
            new()
            {
                UserName = "peter.weber",
                DocumentName = "Liefervertrag_2026.pdf",
                Activity = "Abgelehnt",
                Timestamp = new DateTime(2026, 10, 20, 17, 12, 40)
            },
            new()
            {
                UserName = "sarah.fischer",
                DocumentName = "Projektvertrag.pdf",
                Activity = "Freigegeben",
                Timestamp = new DateTime(2026, 10, 20, 10, 05, 12)
            },
            new()
            {
                UserName = "max.mustermann",
                DocumentName = "Rechnung_2026_079.pdf",
                Activity = "Unterschrieben",
                Timestamp = new DateTime(2026, 10, 19, 15, 45, 33)
            },
            new()
            {
                UserName = "anna.schmidt",
                DocumentName = "Vertraulichkeitsvereinbarung.pdf",
                Activity = "Erstellt",
                Timestamp = new DateTime(2026, 10, 19, 08, 12, 05)
            }
        };
        }
    }
}