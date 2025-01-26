using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ComponentModel;

namespace ImageHunter.Services.VectorDatabase
{
    public class VectorDatabaseService(IWebHostEnvironment env) : IVectorDatabaseService
    {
        QdrantClient qClient = new QdrantClient("localhost");

        public async Task SaveImagestoDb(List<VectorizedImage> images)
        {
            var qdrantRecords = new List<PointStruct>();

            foreach (var image in images)
            {
                qdrantRecords.Add(new PointStruct()
                {
                    Id = new PointId((uint)new Random().Next(0, 10000000)),
                    Vectors = image.Vectors,
                    Payload =
                    {
                        ["name"] = image.FileName
                    }
                });
            }

            await qClient.UpsertAsync("images", qdrantRecords);
        }

        public async Task<List<VectorizedImage>> GetAllImages()
        {
            var results = await qClient.QueryAsync(
                collectionName: "images",
                limit: 100
            );

            List<VectorizedImage> images = new();

            //var fileNames = Directory.GetFiles(env.WebRootPath + "\\images").ToList();

            foreach (var image in results)
            {
                images.Add(new VectorizedImage() { ImagePath = $"/images/{image.Payload["name"].StringValue}" });
            }

            return images;
        }

        public async Task<IReadOnlyList<ScoredPoint>> SearchImages(float[] vector, ulong limit)
        {
            return await qClient.QueryAsync(
                collectionName: "images",
                query: vector,
                limit: limit
            );
        }

        public async Task TryCreateDb()
        {
            try
            {
                await qClient.CreateCollectionAsync("images", new VectorParams { Size = 1024, Distance = Distance.Cosine });
            }
            catch (Exception e)
            {
                // already exists
            }
        }
    }
}
