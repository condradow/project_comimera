using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebFormular.Classes;

namespace WebFormular.Components.Pages
{
    public partial class CreateDocument
    {
        private List<DocumentBase> Documents { get; set; } =
 [
     new()
    {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.Now.AddDays(-10),
        CreatedBy = "Max Mustermann",
        ModifiedOn = DateTime.Now.AddDays(-2),
        ModifiedBy = "Max Mustermann",
        IsInUntis = true,
        ApprovedBySubstitue = true,
        ApprovedByPrincipal = true,
        Title = "Klassenfahrt Berlin",
        Description = "Antrag und Planung für die Klassenfahrt nach Berlin.",
        Elements = []
    },
    new()
    {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.Now.AddDays(-8),
        CreatedBy = "Anna Schmidt",
        ModifiedOn = DateTime.Now.AddDays(-5),
        ModifiedBy = "Anna Schmidt",
        IsInUntis = true,
        ApprovedBySubstitue = true,
        ApprovedByPrincipal = false,
        Title = "Fortbildung",
        Description = "Teilnahme an einer externen Lehrerfortbildung.",
        Elements = []
    },
    new()
    {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.Now.AddDays(-6),
        CreatedBy = "Thomas Müller",
        ModifiedOn = DateTime.Now.AddDays(-3),
        ModifiedBy = "Sekretariat",
        IsInUntis = false,
        ApprovedBySubstitue = true,
        ApprovedByPrincipal = false,
        Title = "Dienstliche Tätigkeit",
        Description = "Anderweitige dienstliche Tätigkeit auf Anordnung der Schulleitung.",
        Elements = []
    },
    new()
    {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.Now.AddDays(-4),
        CreatedBy = "Lisa Weber",
        ModifiedOn = DateTime.Now.AddDays(-1),
        ModifiedBy = "Lisa Weber",
        IsInUntis = false,
        ApprovedBySubstitue = false,
        ApprovedByPrincipal = false,
        Title = "Schulveranstaltung",
        Description = "Schulveranstaltung an einem außerschulischen Lernort.",
        Elements = []
    },
    new()
    {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.Now.AddDays(-2),
        CreatedBy = "Michael Becker",
        ModifiedOn = DateTime.Now,
        ModifiedBy = "Schulleitung",
        IsInUntis = true,
        ApprovedBySubstitue = true,
        ApprovedByPrincipal = true,
        Title = "Exkursion Saarbrücken",
        Description = "Tagesexkursion mit der Klasse nach Saarbrücken.",
        Elements = []
    }
 ];
        private DocumentBase? SelectedDocument;
        private void SelectRow(TableRowClickEventArgs<DocumentBase> args)
        {
            SelectedDocument = args.Item;
        }

        private void OpenDocument(DocumentBase document)
        {
            NavigationManager.NavigateTo($"/documents/{document.Id}");
        }

        private void RowEvents(TableRowClickEventArgs<DocumentBase> args)
        {
            if (args.MouseEventArgs.Detail == 2)
            {
                OpenDocument(args.Item);
            }
        }

        private string RowClassFunc(DocumentBase document, int rowNumber)
        {
            if (SelectedDocument?.Id == document.Id)
            {
                return "mud-info";
            }

            return "document-row";
        }
    }
}