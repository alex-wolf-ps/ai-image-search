using Qdrant.Client.Grpc;

namespace ImageHunter.Services
{
    public interface IVectorDatabaseService
    {
        Task SaveImagestoDb(List<VectorizedImage> images);
        Task<List<VectorizedImage>> SearchImages(float[] vector, int limit);
        Task TryCreateDb();
        Task<List<VectorizedImage>> GetAllImages();
    }
}
