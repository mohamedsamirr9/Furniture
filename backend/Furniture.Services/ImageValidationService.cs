using Furniture.Servises_Abstraction;
using System.Net.Http.Headers;
using System.Text.Json;
using Furniture.shared.Dtos.ProductDtos;

namespace Furniture.Services
{
    public class ImageValidationService : IImageValidationService
    {
        private readonly HttpClient _httpClient;

        public ImageValidationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ImageValidationService");
            
        }

        public async Task<ImageValidationResult> ValidateAsync(
            Stream imageStream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

           
            var response = await _httpClient.PostAsync("/validate-image", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception(
                    $"Image validation service error: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ImageValidationResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? throw new Exception(
                "Invalid response from image validation service");
        }

        public async Task<ImageValidationSummary> ValidateUrlsAsync(
            IEnumerable<string> imageUrls)
        {
            var urls = imageUrls?.ToList();
            if (urls == null || !urls.Any())
                return new ImageValidationSummary { AllApproved = true };

            var results = new List<ImageUrlValidationResult>();

            foreach (var url in urls)
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(url);
                using var stream = new MemoryStream(imageBytes);

                var fileName = Path.GetFileName(new Uri(url).LocalPath);

                var result = await ValidateAsync(stream, fileName, "image/jpeg");

                results.Add(new ImageUrlValidationResult
                {
                    Url = url,
                    Decision = result.Decision,
                    AiProbability = result.AiProbability
                });
            }

            return new ImageValidationSummary
            {
                AllApproved = results.All(r => r.Decision == "approve"),
                Results = results
            };
        }
    }
}