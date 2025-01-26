namespace ImageHunter.Services
{
    public interface IVisionService
    {
        Task<VectorizedResponse> Vectorize(string prompt);
        Task<VectorizedResponse> Vectorize(string imageName, byte[] imageBytes);
    }
}