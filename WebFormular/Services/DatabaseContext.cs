namespace WebFormular.Services
{
    using Microsoft.Data.SqlClient;
    using WebFormular.Classes;

    public class DatabaseContext
    {
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
            const string query = @"
                   
                SELECT 
                    Id
        , CreatedOn
        , CreatedBy
        , ModifiedOn
        , ModifiedBy
        , InUntis
        , ApprovedBySubstitu
        , ApprovedByPrincipal
        , Title        
        , Description
        
        FROM DBO.Documents
                

                ";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();
            List<DocumentBase> documents = [];
            while (await reader.ReadAsync())
            {
                documents.Add(new DocumentBase
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
                    Description = reader.GetString(9)
                });
            }

            return documents;
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
