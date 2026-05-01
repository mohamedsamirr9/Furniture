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

    public RecommendationService(
        IUnitOfWork unitOfWork,
        IHttpClientFactory factory,
        IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _httpClient = factory.CreateClient("AIService");
        
    }

    public async Task GenerateAndSaveEmbeddingAsync(
        int productId, string imageUrl, string description)
    {
        var payload = new { product_id = productId, image_url = imageUrl, description };

        var response = await _httpClient.PostAsJsonAsync("/embed/product", payload);
        if (!response.IsSuccessStatusCode) return;

        var result  = await response.Content.ReadFromJsonAsync<EmbedProductResponse>();
        var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(productId);
        if (product is null || result is null) return;

        product.EmbeddingVector = JsonSerializer.Serialize(result.Embedding);
        _unitOfWork.GetRepository<Product, int>().Update(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SaveUserQuizAsync(
        string userId, string style, string color, string roomSize, string budget)
    {
        var payload  = new { style, color, room_size = roomSize, budget };

        var response = await _httpClient.PostAsJsonAsync("/embed/quiz", payload);
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

    public async Task<List<ProductRecommendationDto>> GetRecommendationsAsync(
        string userId, int topK = 5)
    {
        var spec = new UserPreferenceByUserIdSpecification(userId);
        var pref = await _unitOfWork.GetRepository<UserPreference, int>().GetByIdAsync(spec);

        if (string.IsNullOrWhiteSpace(pref?.EmbeddingVector))
            return new List<ProductRecommendationDto>();

        var payload = new
        {
            embedding = JsonSerializer.Deserialize<List<float>>(pref.EmbeddingVector),
            top_k = topK
        };

        var response = await _httpClient.PostAsJsonAsync("/recommend", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RecommendResponse>();
        var productIds = result?.Recommendations.Select(r => r.ProductId).ToList() ?? new();

        if (!productIds.Any()) return new();

        var productSpec = new ProductsByIdsSpecification(productIds);
        var products = await _unitOfWork.GetRepository<Product, int>()
            .GetAllAsync(productSpec);

        return products.Select(p => new ProductRecommendationDto
        {
            Id = p.Id,
            Name = p.NameEn,
            Price = p.Price,
            ImageUrl = p.Images.FirstOrDefault()?.ImageUrl
        }).ToList();
    }

    public async Task UpdateUserEmbeddingAsync(
        string userId, int productId, string actionType)
    {
        var repo = _unitOfWork.GetRepository<UserPreference, int>();

        var spec = new UserPreferenceByUserIdSpecification(userId);
        var pref = await repo.GetByIdAsync(spec);

        var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(productId);

        if (product?.EmbeddingVector is null)
            return;

        if (pref is null)
        {
            pref = new UserPreference
            {
                UserId = userId,
                EmbeddingVector = product.EmbeddingVector,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(pref);
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(pref.EmbeddingVector))
        {
            pref.EmbeddingVector = product.EmbeddingVector;
            pref.UpdatedAt = DateTime.UtcNow;

            repo.Update(pref);
            await _unitOfWork.SaveChangesAsync();
            return;
        }
        var payload = new
        {
            user_embedding = JsonSerializer.Deserialize<List<float>>(pref.EmbeddingVector),
            product_embedding = JsonSerializer.Deserialize<List<float>>(product.EmbeddingVector),
            action_type = actionType
        };

        var response = await _httpClient.PostAsJsonAsync("/embed/update", payload);
        if (!response.IsSuccessStatusCode) return;

        var result = await response.Content.ReadFromJsonAsync<UpdateEmbeddingResponse>();
        if (result is null) return;

        pref.EmbeddingVector = JsonSerializer.Serialize(result.Embedding);
        pref.UpdatedAt = DateTime.UtcNow;

        repo.Update(pref);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IndexAllProductsAsync(bool onlyMissing = false)
    {
        var spec     = new ProductsWithImagesSpecification();
        var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(spec);

        if (onlyMissing)
        {
            products = products.Where(p => string.IsNullOrEmpty(p.EmbeddingVector));
        }

        foreach (var product in products)
        {
            var firstImage = product.Images.FirstOrDefault()?.ImageUrl;
            if (firstImage is null) continue;

            await GenerateAndSaveEmbeddingAsync(
                product.Id, firstImage, product.DescriptionEn);
        }
    }

    public async Task DeleteProductEmbeddingAsync(int productId)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/delete/product", new { product_id = productId });

        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"Failed to delete embedding for product {productId}");
    }
    
    public async Task<bool> HasCompletedQuizAsync(string userId)
    {
        var repo = _unitOfWork.GetRepository<UserPreference, int>();
        var spec = new UserPreferenceByUserIdSpecification(userId);
        var pref = await repo.GetByIdAsync(spec);

        return pref is not null
               && !string.IsNullOrWhiteSpace(pref.Style)
               && !string.IsNullOrWhiteSpace(pref.Color)
               && !string.IsNullOrWhiteSpace(pref.RoomSize)
               && !string.IsNullOrWhiteSpace(pref.Budget);
    }
}