using Furniture.shared.Dtos;
using Furniture.shared.Dtos.Cart;

namespace Furniture.Servises_Abstraction
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task<CartDto> AddToCartAsync(string userId, AddToCartDto dto);
        Task<CartDto> UpdateCartItemAsync(string userId, int productId, UpdateCartItemDto dto);
        Task<CartDto> RemoveFromCartAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}