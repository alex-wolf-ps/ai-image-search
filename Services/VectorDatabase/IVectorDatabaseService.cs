using Qdrant.Client.Grpc;

namespace ImageHunter.Services
{
    public interface IVectorDatabaseService
    {
        Task SaveImagestoDb(List<VectorizedImage> images);
        Task<IReadOnlyList<ScoredPoint>> SearchImages(float[] vector, ulong limit);
        Task TryCreateDb();
        Task<List<VectorizedImage>> GetAllImages();
    }
}
