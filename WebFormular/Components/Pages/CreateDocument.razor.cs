using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebFormular.Classes;
using WebFormular.Classes.Elements.ApplicantLessonChangeRequest;
using WebFormular.Classes.Elements.FinalSigningElement;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClass;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClassAndTeacher;
using WebFormular.Classes.Elements.OfficialDutyDuringStudentAbsence;
using WebFormular.Classes.Elements.OffSiteSchoolEvent;
using WebFormular.Classes.Elements.OtherSchoolEvent;
using WebFormular.Classes.Elements.TeachingAbsenceReason;

namespace WebFormular.Components.Pages
{
    public partial class CreateDocument
    {
        private List<DocumentBase> Documents { get; set; }= [];
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

        public Boolean FillOutMode { get; set; } = false;

        public void ToggleFillOutMode()
        {
            FillOutMode = !FillOutMode;
        }
        private string RowClassFunc(DocumentBase document, int rowNumber)
        {
            if (SelectedDocument?.Id == document.Id)
            {
                return "mud-info";
            }

            return "document-row";
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.Documents = await DbContext.GetAllAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}