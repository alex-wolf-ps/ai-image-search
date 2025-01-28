namespace ImageHunter.Services
{
    public interface IVisionService
    {
        Task<VectorizedResponse> VectorizeText(string prompt);
        Task<VectorizedResponse> VectorizeImage(byte[] imageBytes);
    }
}