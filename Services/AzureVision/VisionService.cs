using System.Text.Json;
using System.Text;

namespace ImageHunter.Services
{
    public class VisionService : IVisionService
    {
        HttpClient client;

        public VisionService(IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateClient("vision");
        }

        public async Task<VectorizedResponse> Vectorize(string prompt)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/computervision/retrieval:vectorizeText?api-version=2024-02-01&model-version=2023-04-15");
            request.Content = new StringContent(JsonSerializer.Serialize(new { text = prompt }), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VectorizedResponse>(responseString);
        }

        public async Task<VectorizedResponse> Vectorize(string imageName, byte[] imageBytes)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/computervision/retrieval:vectorizeImage?api-version=2024-02-01&model-version=2023-04-15");
            var binaryContent = new ByteArrayContent(imageBytes);
            binaryContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            request.Content = binaryContent;

            var response = await client.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VectorizedResponse>(responseString);
        }

    }
}
