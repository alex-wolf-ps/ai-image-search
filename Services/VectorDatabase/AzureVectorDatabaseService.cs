using ChromaDB.Client;
using Microsoft.Data.SqlClient;
using MudBlazor;
using MudBlazor.Extensions;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace ImageHunter.Services.VectorDatabase
{
    public class AzureVectorDatabaseService(
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory) : IVectorDatabaseService
    {
        private string connStr = "Server=tcp:dbchatserver2.database.windows.net,1433;Initial Catalog=codewolfimages;Persist Security Info=False;User ID=arw002;Password=Dotnetcore!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        public async Task SaveImagestoDb(List<VectorizedImage> images)
        {
            using (SqlConnection connection = new SqlConnection(connStr))
            { 
                List<SqlCommand> commands = new();
                foreach (var image in images)
                {
                    // Vector is inserted in the column '[VectorShort] VECTOR(3)  NULL' 
                    string sql = @$"INSERT INTO [dbo].[images] (id, name, vectors) 
                    VALUES ('{new Random().Next()}', '{image.FileName}', '{JsonSerializer.Serialize(image.Vectors)}')"; 
                    
                    // Insert vector as string. Note JSON array.    
                    commands.Add(new SqlCommand(sql, connection)); 
                }

                connection.Open();
                foreach (var command in commands)
                {
                    await command.ExecuteNonQueryAsync(); 
                }
                connection. Close();
            }
        }

        public async Task<List<VectorizedImage>> GetAllImages()
        {
            List<(int Id, string name, string vectors)> rows = new();

            using (SqlConnection connection = new SqlConnection(connStr))
            { 
                string sql = @$"SELECT * from [dbo].[images]";
                SqlCommand command = new SqlCommand(sql, connection);
                
                connection.Open();
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        (int Id, string Name, string vectors) row = new(
                            reader.GetInt32(reader.GetOrdinal("Id")), 
                            reader.GetString(reader.GetOrdinal("Name")), 
                            reader.GetString(reader.GetOrdinal("Vectors"))
                        ); 
                        
                        rows.Add(row);
                    }
                }
                connection. Close();
            }

            //var fileNames = Directory.GetFiles(env.WebRootPath + "\\images").ToList();
            List<VectorizedImage> images = new();
            foreach (var item in rows)
            {
                images.Add(new VectorizedImage() { ImagePath = $"/images/{item.name}" });
            }

            return images;
        }

        public async Task<List<VectorizedImage>> SearchImages(float[] vector, int limit)
        {
            
            List<VectorizedImage> images = new();
            List<(int Id, string name, double Distance)> rows = new();

            using (SqlConnection connection = new SqlConnection(connStr))
            { 
                string sql = @$"SELECT TOP({limit}) id, name, 
                VECTOR_DISTANCE('cosine', CAST(@Embedding AS Vector(1024)), vectors) 
                AS Distance from [dbo].[images] ORDER BY Distance";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Embedding", JsonSerializer.Serialize(vector));

                connection.Open();
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        VectorizedImage image = new VectorizedImage()
                        {
                            FileName = reader.GetString(reader.GetOrdinal("Name")),
                            Score = (float)reader.GetDouble(reader.GetOrdinal("Distance")),
                            ImagePath = $"/images/{reader.GetString(reader.GetOrdinal("Name"))}"
                        };
                        images.Add(image);
                    }
                }
                connection. Close();
            }

            //var fileNames = Directory.GetFiles(env.WebRootPath + "\\images").ToList();
            foreach (var item in rows)
            {
                images.Add(new VectorizedImage() { ImagePath = $"/images/{item.name}" });
            }

            return images;
        }

        public async Task TryCreateDb()
        {
            try
            {
                // We don't use this for Azure SQL
            }
            catch (Exception e)
            {
                // already exists
            }
        }

    }
}
