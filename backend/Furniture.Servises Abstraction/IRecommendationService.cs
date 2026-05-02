using Furniture.shared.Dtos.Recommendation;

namespace Furniture.Servises_Abstraction;

public interface IRecommendationService
{
    Task GenerateAndSaveEmbeddingAsync(int productId, string imageUrl, string description);
    Task SaveUserQuizAsync(string userId, string style, string color, string roomSize, string budget);
    Task<List<ProductRecommendationDto>> GetRecommendationsAsync(string userId, int topK = 5);
    Task UpdateUserEmbeddingAsync(string userId, int productId, string actionType);
    Task IndexAllProductsAsync(bool onlyMissing = false);
    Task DeleteProductEmbeddingAsync(int productId);
    Task<bool> HasCompletedQuizAsync(string userId);

}

