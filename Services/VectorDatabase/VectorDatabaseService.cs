using ChromaDB.Client;
using MudBlazor;
using MudBlazor.Extensions;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ComponentModel;

namespace ImageHunter.Services.VectorDatabase
{
    public class VectorDatabaseService(IWebHostEnvironment env, ChromaClient chromaClient, ChromaCollectionClient chromaCollection) : IVectorDatabaseService
    {
        QdrantClient qClient = new QdrantClient("localhost");

        public async Task SaveImagestoDb(List<VectorizedImage> images)
        {
            List<string> imageIds = new();
            List<ReadOnlyMemory<float>> imageVectors = new();
            List<Dictionary<string,object>> imageMetadata = new();

            foreach (var image in images)
            {
                imageIds.Add(new Random().Next().ToString());
                imageVectors.Add(image.Vectors);
                imageMetadata.Add(new Dictionary<string, object> { ["Name"] = image.FileName });
            }

            await chromaCollection.Add(imageIds, imageVectors, imageMetadata);
        }

        public async Task<List<VectorizedImage>> GetAllImages()
        {
            List<VectorizedImage> images = new();

            var collection = await chromaCollection.Get();

            //var fileNames = Directory.GetFiles(env.WebRootPath + "\\images").ToList();

            foreach (var item in collection)
            {
                images.Add(new VectorizedImage() { ImagePath = $"/images/{item.Metadata["Name"]}" });
            }

            return images;
        }

        public async Task<List<VectorizedImage>> SearchImages(float[] vector, int limit)
        {
            List<ReadOnlyMemory<float>> imageVectors = [
                vector
            ];

            var queryResults = await chromaCollection.Query(
                queryEmbeddings: imageVectors,
                limit,
                include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances | ChromaQueryInclude.Embeddings);

            List<VectorizedImage> images = new();

            //var fileNames = Directory.GetFiles(env.WebRootPath + "\\images").ToList();

            foreach (var result in queryResults)
            {
                foreach(var item in result)
                {
                    images.Add(new VectorizedImage() { Score = item.Distance, FileName = item.Metadata["Name"].ToString(), ImagePath = $"/images/{item.Metadata["Name"]}" });
                }
            }

            return images.OrderByDescending(x => x.Score).ToList();
        }

        public async Task TryCreateDb()
        {
            try
            {
                await chromaClient.GetOrCreateCollection("images");
            }
            catch (Exception e)
            {
                // already exists
            }
        }
    }
}
