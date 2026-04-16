using System.Net.Http.Json;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications.SearchWithImage;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SearchWithImage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Furniture.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SearchService> _logger;

        private const string PythonServiceUrl = "https://raniaxyz-furniture-visual-search.hf.space";

        public SearchService(
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            ILogger<SearchService> logger)
        {
            _unitOfWork  = unitOfWork;
            _httpClient  = httpClientFactory.CreateClient("PythonService");
            _logger      = logger;
        }

        public async Task<IEnumerable<ProductSearchResultDto>> SearchByImageAsync(IFormFile image, int topK = 5)
        {
            var pythonResults = await CallPythonSearchAsync(image, topK);
    
            foreach (var r in pythonResults)
            {
                Console.WriteLine($"  ProductId: '{r.ProductId}', Similarity: {r.Similarity}");
            }

            if (!pythonResults.Any())
                return Enumerable.Empty<ProductSearchResultDto>();

            var productIds = pythonResults
                .Where(r => r != null && !string.IsNullOrEmpty(r.ProductId))
                .Select(r => ParseProductId(r.ProductId))
                .Where(id => id > 0)
                .ToList();


            var spec = new ProductsByIdsSpecification(productIds);
            var products = await _unitOfWork
                .GetRepository<Product, int>()
                .GetAllAsync(spec);


            return pythonResults
                .Select(pr =>
                {
                    var productId = ParseProductId(pr.ProductId);
                    var product = products.FirstOrDefault(p => p.Id == productId);
                    if (product == null) return null;

                    return new ProductSearchResultDto
                    {
                        ProductId = product.Id,
                        Name = product.NameEn,
                        Price = product.Price,
                        Similarity = pr.Similarity,
                        ImageUrl = product.Images.FirstOrDefault()?.ImageUrl
                    };
                })
                .Where(r => r != null)!;
        }
        public async Task<BuildIndexResponseDto> BuildIndexAsync()
        {
            var images = await _unitOfWork
                .GetRepository<ProductImage, int>()
                .GetAllAsync();

            if (!images.Any())
                throw new InvalidOperationException("No product images found in database.");

            var payload = new BuildIndexRequestDto
            {
                Products = images.Select(img => new ProductImageDto
                {
                    ProductId = img.ProductId,
                    ImageUrl = img.ImageUrl.Contains("?")
                        ? img.ImageUrl
                        : img.ImageUrl + "?w=400&q=80"
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{PythonServiceUrl}/build-index-from-urls", payload);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Python service error: {responseContent}");

            return System.Text.Json.JsonSerializer.Deserialize<BuildIndexResponseDto>(responseContent)
                   ?? throw new Exception("Empty response from Python service.");
        }
        private async Task<List<PythonSearchResult>> CallPythonSearchAsync(IFormFile image, int topK)
        {
            try
            {
                using var content     = new MultipartFormDataContent();
                using var stream      = image.OpenReadStream();
                using var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        image.ContentType ?? "image/jpeg");
                content.Add(fileContent, "file", image.FileName);

                var response = await _httpClient.PostAsync(
                    $"{PythonServiceUrl}/search?top_k={topK}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Python search returned {Status}", response.StatusCode);
                    return new();
                }

                var result = await response.Content
                    .ReadFromJsonAsync<PythonSearchResponse>();

                return result?.Results ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reach Python search service.");
                throw new Exception("Search service unavailable.", ex);
            }
        }

        private static int ParseProductId(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return 0;
            if (productId.Contains('_'))
            {
                var parts = productId.Split('_');
                if (int.TryParse(parts.Last(), out var id)) return id;
            }
            if (int.TryParse(productId, out var directId)) return directId;
            return 0;
        }    }
}