using System.Net.Http.Json;
using System.Text.Json;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications.AIRecommendation;
using Furniture.Services.Specifications.SearchWithImage;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.AIResponse;
using Furniture.shared.Dtos.Recommendation;
using Microsoft.Extensions.Configuration;

public class RecommendationService : IRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly string _aiBaseUrl;

    public RecommendationService(IUnitOfWork unitOfWork, IHttpClientFactory factory, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _httpClient = factory.CreateClient("AIService");
        _aiBaseUrl  = config["AIRecommendation:BaseUrl"]!;
    }

    public async Task GenerateAndSaveEmbeddingAsync(int productId, string imageUrl, string description)
    {
        var payload  = new { product_id = productId, image_url = imageUrl, description };
        var response = await _httpClient.PostAsJsonAsync($"{_aiBaseUrl}/embed/product", payload);
        if (!response.IsSuccessStatusCode) return;

        var result  = await response.Content.ReadFromJsonAsync<EmbedProductResponse>();
        var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(productId);
        if (product is null || result is null) return;

        product.EmbeddingVector = JsonSerializer.Serialize(result.Embedding);
        _unitOfWork.GetRepository<Product, int>().Update(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SaveUserQuizAsync(string userId, string style, string color, string roomSize, string budget)
    {
        var payload  = new { style, color, room_size = roomSize, budget };
        var response = await _httpClient.PostAsJsonAsync($"{_aiBaseUrl}/embed/quiz", payload);
        if (!response.IsSuccessStatusCode) return;

        var result = await response.Content.ReadFromJsonAsync<EmbedQuizResponse>();
        if (result is null) return;

        var repo = _unitOfWork.GetRepository<UserPreference, int>();
        var spec = new UserPreferenceByUserIdSpecification(userId);
        var pref = await repo.GetByIdAsync(spec);

        if (pref is null)
        {
            pref = new UserPreference { UserId = userId };
            await repo.AddAsync(pref);
        }

        pref.EmbeddingVector = JsonSerializer.Serialize(result.Embedding);
        pref.Style     = style;
        pref.Color     = color;
        pref.RoomSize  = roomSize;
        pref.Budget    = budget;
        pref.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<ProductRecommendationDto>> GetRecommendationsAsync(string userId, int topK = 5)
    {
        var spec = new UserPreferenceByUserIdSpecification(userId);
        var pref = await _unitOfWork.GetRepository<UserPreference, int>().GetByIdAsync(spec);

        if (pref?.EmbeddingVector is null)
            throw new Exception("Complete the quiz first.");

        var payload  = new { embedding = JsonSerializer.Deserialize<List<float>>(pref.EmbeddingVector), top_k = topK };
        var response = await _httpClient.PostAsJsonAsync($"{_aiBaseUrl}/recommend", payload);
        response.EnsureSuccessStatusCode();

        var result     = await response.Content.ReadFromJsonAsync<RecommendResponse>();
        var productIds = result?.Recommendations.Select(r => r.ProductId).ToList() ?? new();

        if (!productIds.Any()) return new();

        var productSpec = new ProductsByIdsSpecification(productIds);
        var products    = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(productSpec);

        return products.Select(p => new ProductRecommendationDto
        {
            Id       = p.Id,
            Name     = p.NameEn,
            Price    = p.Price,
            ImageUrl = p.Images.FirstOrDefault()?.ImageUrl
        }).ToList();
    }

    public async Task UpdateUserEmbeddingAsync(string userId, int productId, string actionType)
    {
        var spec    = new UserPreferenceByUserIdSpecification(userId);
        var pref    = await _unitOfWork.GetRepository<UserPreference, int>().GetByIdAsync(spec);
        var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(productId);

        if (pref?.EmbeddingVector is null || product?.EmbeddingVector is null) return;

        var payload = new
        {
            user_embedding    = JsonSerializer.Deserialize<List<float>>(pref.EmbeddingVector),
            product_embedding = JsonSerializer.Deserialize<List<float>>(product.EmbeddingVector),
            action_type       = actionType
        };

        var response = await _httpClient.PostAsJsonAsync($"{_aiBaseUrl}/embed/update", payload);
        if (!response.IsSuccessStatusCode) return;

        var result = await response.Content.ReadFromJsonAsync<UpdateEmbeddingResponse>();
        if (result is null) return;

        pref.EmbeddingVector = JsonSerializer.Serialize(result.Embedding);
        pref.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IndexAllProductsAsync()
    {
        var spec     = new ProductsWithImagesSpecification();
        var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(spec);

        foreach (var product in products)
        {
            var firstImage = product.Images.FirstOrDefault()?.ImageUrl;
            if (firstImage is null) continue;

            await GenerateAndSaveEmbeddingAsync(product.Id, firstImage, product.DescriptionEn);
        }
    }

    public async Task DeleteProductEmbeddingAsync(int productId)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_aiBaseUrl}/delete/product", new { product_id = productId });

        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"Failed to delete embedding for product {productId}");
    }
}