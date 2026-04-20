using Furniture.Domain.Models.Enum;
using Furniture.shared.Dtos.Order;

namespace Furniture.Servises_Abstraction;

public interface IOrderService
{
    

    #region User

    Task<List<OrderDTO>> GetUserOrdersAsync(string userId);
    Task<PaginatedOrdersDTO> GetUserOrdersPaginatedAsync(string userId, int pageIndex, int pageSize);
    Task<OrderDTO?> GetOrderByIdAsync(int orderId, string userId);
    Task<OrderResponseDTO> CreateOrderFromCartAsync(string userId, CreateOrderDTO createOrderDTO);
    Task<OrderResponseDTO> CreateOrderFromOfferAsync(string userId, CreateOrderFromOfferDTO createOrderFromOfferDTO);
    Task<bool> CancelOrderAsync(int orderId, string userId);

    #endregion
        
    #region Admin

    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string adminId);
    Task<List<OrderDTO>> GetOrdersByStatusAsync(OrderStatus status);
    Task<OrderDTO?> GetOrderByIdForAdminAsync(int orderId);
    Task<PaginatedOrdersDTO> GetAllOrdersPaginatedAsync(int pageIndex, int pageSize);

    #endregion

    #region Seller

    Task<List<OrderDTO>> GetOrdersForSellerAsync(string sellerId);
    Task<OrderDTO?> GetOrderByIdForSellerAsync(int orderId, string sellerId);

    #endregion
}