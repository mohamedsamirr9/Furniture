using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;

namespace Furniture.Services
{
    public class ImageValidationService : IImageValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _validationApiUrl;

        public ImageValidationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _validationApiUrl = configuration["ImageValidation:ApiUrl"] 
                                ?? "http://localhost:8001/validate-image";
        }

        
        public async Task<ImageValidationResultDto> ValidateImageAsync(
            Stream imageStream, 
            string fileName)
        {
            var memoryStream = new MemoryStream();
    
            if (imageStream.CanSeek)
                imageStream.Position = 0;
    
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(memoryStream);
    
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync(_validationApiUrl, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
    
            var result = JsonSerializer.Deserialize<ImageValidationResultDto>(
                jsonResponse, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? throw new InvalidOperationException(
                "Failed to deserialize validation response"
            );
        }
    }
}