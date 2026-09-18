using WebFormular.Classes.Elements.ApplicantLessonChangeRequest;
using WebFormular.Classes.Elements.FinalSigningElement;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClass;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClassAndTeacher;
using WebFormular.Classes.Elements.OfficialDutyDuringStudentAbsence;
using WebFormular.Classes.Elements.OffSiteSchoolEvent;
using WebFormular.Classes.Elements.OtherSchoolEvent;
using WebFormular.Classes.Elements.TeachingAbsenceReason;

namespace WebFormular.Services
{
    using Microsoft.Data.SqlClient;
    using WebFormular.Classes;

    public class DatabaseContext
    {   
        //mockup data
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
        private readonly string _connectionString;

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
          

        }

        // CREATE
        public async Task<int> CreateAsync(
            int documentId,
            string elementType,
            int sortOrder,
            string json)
        {
            const string sql = @"
            INSERT INTO DocumentElements
                (DocumentId, ElementType, SortOrder, Data)
            OUTPUT INSERTED.Id
            VALUES
                (@DocumentId, @ElementType, @SortOrder, @Data)
            ";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@DocumentId", documentId);
            command.Parameters.AddWithValue("@ElementType", elementType);
            command.Parameters.AddWithValue("@SortOrder", sortOrder);
            command.Parameters.AddWithValue("@Data", json);

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        public async Task<List<DocumentBase>> GetAllAsync()
        {
//             const string query = @"
//                    
//                 SELECT 
//                     Id
//         , CreatedOn
//         , CreatedBy
//         , ModifiedOn
//         , ModifiedBy
//         , InUntis
//         , ApprovedBySubstitu
//         , ApprovedByPrincipal
//         , Title        
//         , Description
//         
//         FROM DBO.Documents
//                 
//
//                 ";
//
//             await using var connection = new SqlConnection(_connectionString);
//             await connection.OpenAsync();
//
//             await using var command = new SqlCommand(query, connection);
//             await using var reader = await command.ExecuteReaderAsync();
//             List<DocumentBase> documents = [];
//             while (await reader.ReadAsync())
//             {
//                 documents.Add(new DocumentBase
//                 {
//                     Id = reader.GetGuid(0),
//                     CreatedOn = reader.GetDateTime(1),
//                     CreatedBy = reader.GetString(2),
//                     ModifiedOn = reader.GetDateTime(3),
//                     ModifiedBy = reader.GetString(4),
//                     IsInUntis = reader.GetBoolean(5),
//                     ApprovedBySubstitue = reader.GetBoolean(6),
//                     ApprovedByPrincipal = reader.GetBoolean(7),
//                     Title = reader.GetString(8),
//                     Description = reader.GetString(9)
//                 });
//             }

            return this.Documents;
        }



        // READ
        public async Task<List<DocumentBase>> GetByDocumentIdAsync(int documentId)
        {
            var elements = new List<DocumentBase>();

            const string sql = """
            SELECT Id, DocumentId, ElementType, SortOrder, Data
            FROM DocumentElements
            WHERE DocumentId = @DocumentId
            ORDER BY SortOrder
            """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DocumentId", documentId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                elements.Add(new DocumentBase
                {
                    Id = reader.GetGuid(0),
                    CreatedOn = reader.GetDateTime(1),
                    CreatedBy = reader.GetString(2),
                    ModifiedOn = reader.GetDateTime(3),
                    ModifiedBy = reader.GetString(4),
                    IsInUntis = reader.GetBoolean(5),
                    ApprovedBySubstitue = reader.GetBoolean(6),
                    ApprovedByPrincipal = reader.GetBoolean(7),
                    Title = reader.GetString(8),


                });
            }

            //TODO elements
            return elements;
        }


        // UPDATE
        public async Task UpdateAsync(
            int id,
            string elementType,
            int sortOrder,
            string json)
        {
            const string sql = """
            UPDATE DocumentElements
            SET
                ElementType = @ElementType,
                SortOrder = @SortOrder,
                Data = @Data
            WHERE Id = @Id
            """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@ElementType", elementType);
            command.Parameters.AddWithValue("@SortOrder", sortOrder);
            command.Parameters.AddWithValue("@Data", json);

            await command.ExecuteNonQueryAsync();
        }


        // DELETE
        public async Task DeleteAsync(int id)
        {
            const string sql = """
            DELETE FROM DocumentElements
            WHERE Id = @Id
            """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
