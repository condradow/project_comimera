using WebFormular.Classes;

namespace WebFormular.Components.Pages;

public partial class DocumentOverview
{
    private List<DocumentBase> Documents { get; set; } = [];
    private string SearchString { get; set; } = string.Empty;

    private Func<DocumentBase, bool> QuickFilter => document =>
    {
        if (string.IsNullOrWhiteSpace(SearchString))
            return true;

        if (document.Title?.Contains(
                SearchString,
                StringComparison.OrdinalIgnoreCase) == true)
            return true;

        if (document.Description?.Contains(
                SearchString,
                StringComparison.OrdinalIgnoreCase) == true)
            return true;

        if (document.CreatedBy?.Contains(
                SearchString,
                StringComparison.OrdinalIgnoreCase) == true)
            return true;

        if (document.ModifiedBy?.Contains(
                SearchString,
                StringComparison.OrdinalIgnoreCase) == true)
            return true;

        return false;
    };

    private async Task DeleteDocument(DocumentBase document)
    {
        // Später DatabaseService aufrufen

        // await Database.DeleteDocumentAsync(document.Id);

        Documents.Remove(document);

        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            this.Documents = await DbContext.GetAllAsync();

        }
        catch (Exception)
        {
            Console.WriteLine("test");
        }
    }
}