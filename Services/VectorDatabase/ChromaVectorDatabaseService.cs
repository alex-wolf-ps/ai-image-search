using ChromaDB.Client;
using MudBlazor;
using MudBlazor.Extensions;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ComponentModel;

namespace ImageHunter.Services.VectorDatabase
{
    public class VectorDatabaseService(
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory) : IVectorDatabaseService
    {
        public async Task SaveImagestoDb(List<VectorizedImage> images)
        {
            var chromaCollection = await GetChromaCollectionClient();

            List<string> imageIds = new();
            List<ReadOnlyMemory<float>> imageVectors = new();
            List<Dictionary<string,object>> imageMetadata = new();

            foreach (var image in images)
            {
                imageIds.Add(new Random().Next().ToString());
                imageVectors.Add(image.Vectors);
                imageMetadata.Add(new Dictionary<string, object> { ["Name"] = image.FileName });
            }

            await chromaCollection.Upsert(imageIds, imageVectors, imageMetadata);
        }

        public async Task<List<VectorizedImage>> GetAllImages()
        {
            var chromaCollection = await GetChromaCollectionClient();

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
            var chromaCollection = await GetChromaCollectionClient();
            
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
                var httpClient = httpClientFactory.CreateClient();
                var configOptions = new ChromaConfigurationOptions(uri: "http://localhost:8000/api/v1/");
                
                var chromaClient = new ChromaClient(configOptions, httpClient);
                var collection = await chromaClient.GetOrCreateCollection("images", metadata: new Dictionary<string, object>
                {
                    { "hnsw:space", "cosine" }
                });
            }
            catch (Exception e)
            {
                // already exists
            }
        }

        private async Task<ChromaCollectionClient> GetChromaCollectionClient()
        {
            var httpClient = httpClientFactory.CreateClient();
            var configOptions = new ChromaConfigurationOptions(uri: "http://localhost:8000/api/v1/");
            
            var chromaClient = new ChromaClient(configOptions, httpClient);
            var collectionClient = await chromaClient.GetOrCreateCollection("images", metadata: new Dictionary<string, object>
            {
                { "hnsw:space", "cosine" }
            });
            
            return new ChromaCollectionClient(collectionClient, configOptions, httpClient);
        }
    }
}
