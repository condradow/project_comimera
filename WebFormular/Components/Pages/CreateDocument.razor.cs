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
        Title = "Unterrichtsvertretung aus dienstlichen Gründen",
        Description = "",
        Elements = [new TeachingAbsenceReason(),new FinalSigningElement()]
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
        Title = "Dienstbefreiung nach §14 Abs. 1 Urlaubsverordnung",
        Description = "",
        Elements = [new TeachingAbsenceReason(),new FinalSigningElement()]
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
        Title = "Dienstbefreiung für Fortbildungsveranstaltung",
        Description = "",
        Elements = [new TeachingAbsenceReason(),new FinalSigningElement()]
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
        Title = "Unterrichtsverlegung aus privaten Gründen",
        Description = "",
        Elements = [new TeachingAbsenceReason(),new ApplicantLessonChangeRequest(),new FinalSigningElement()]
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
        Title = "Unterrichtstausch",
        Description = "",
        Elements = [new TeachingAbsenceReason(),new ApplicantLessonChangeRequest(),new FinalSigningElement()]
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
         Title = "Unterrichtsvertretung wegen Veranstaltun",
         Description = "",
         Elements = [new LessonArrangementForAbsentClassAndTeacher(),new OffSiteSchoolEvent(),new OtherSchoolEvent(),new OfficialDutyDuringStudentAbsence(),new FinalSigningElement()]
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
         Title = "Klasse abwesend ohne Lehrkraft",
         Description = "",
         Elements = [new LessonArrangementForAbsentClass(),new OfficialDutyDuringStudentAbsence(),new FinalSigningElement()]
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
    }
}