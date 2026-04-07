using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Services.Specifications.Order;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Order;

namespace Furniture.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        
        
        #region User 

        
        
        public async Task<List<OrderDTO>> GetUserOrdersAsync(string userId)
        {
            var spec = new OrderSpecifications(userId);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);
            return _mapper.Map<List<OrderDTO>>(orders);
        }

        
        
        public async Task<PaginatedOrdersDTO> GetUserOrdersPaginatedAsync(
            string userId, int pageIndex, int pageSize)
        {
            var spec = new OrderSpecifications(userId, pageIndex, pageSize);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);

            var countSpec = new OrderSpecifications(userId);
            var totalCount = await _unitOfWork.GetRepository<Order, int>()
                .CountAsync(countSpec);

            return new PaginatedOrdersDTO
            {
                Orders = _mapper.Map<List<OrderDTO>>(orders),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        
        
        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId, string userId)
        {
            var spec = new OrderSpecifications(orderId, userId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            return order == null ? null : _mapper.Map<OrderDTO>(order);
        }

        
        
        public async Task<OrderResponseDTO> CreateOrderFromCartAsync(
            string userId, CreateOrderDTO createOrderDTO)
        {
            var cartRepo = _unitOfWork.GetRepository<Cart, int>();
            var cartSpec = new CartWithItemsSpecification(userId);
            var cart = await cartRepo.GetByIdAsync(cartSpec);

            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty!");

            decimal totalPrice = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product == null)
                    throw new InvalidOperationException($"Product with ID {cartItem.ProductId} not found");

                var currentPrice = cartItem.Product.Price;
                totalPrice += currentPrice * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    UnitPrice = currentPrice,
                    Quantity = cartItem.Quantity
                });
            }

            var newOrder = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = createOrderDTO.ShippingAddress,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            await _unitOfWork.GetRepository<Order, int>().AddAsync(newOrder);


            foreach (var item in cart.CartItems.ToList())
            {
                cart.CartItems.Remove(item);
            }

            await _unitOfWork.SaveChangesAsync();

            return new OrderResponseDTO
            {
                OrderId = newOrder.Id,
                TotalPrice = newOrder.TotalPrice,
                OrderDate = newOrder.OrderDate,
                Status = newOrder.Status.ToString(),
                Message = "Order created successfully!"
            };
        }
        
        

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            var spec = new OrderSpecifications(orderId, userId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            if (order == null)
                return false;

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Accepted)
                throw new InvalidOperationException(
                    $"Cannot cancel order with status '{order.Status}'. Only Pending or Accepted orders can be cancelled.");

            order.Status = OrderStatus.Cancelled;  
            _unitOfWork.GetRepository<Order, int>().Update(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        
        #region Admin 

        public async Task<bool> UpdateOrderStatusAsync(
            int orderId, OrderStatus newStatus, string adminId)
        {
            var spec = new OrderSpecifications(orderId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            if (order == null)
                return false;

            ValidateStatusTransition(order.Status, newStatus);

            if (newStatus == OrderStatus.Paid && order.Status != OrderStatus.Paid)
            {
                var productRepo = _unitOfWork.GetRepository<Product, int>();
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity -= item.Quantity;
                        if (item.Product.StockQuantity <= 0)
                        {
                            item.Product.StockQuantity = 0;
                            item.Product.IsAvailable = false;
                        }
                        productRepo.Update(item.Product);
                    }
                }
            }

            order.Status = newStatus;
            _unitOfWork.GetRepository<Order, int>().Update(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<OrderDTO>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var spec = new OrderByStatusSpecification(status);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);
            return _mapper.Map<List<OrderDTO>>(orders);
        }


        public async Task<OrderDTO?> GetOrderByIdForAdminAsync(int orderId)
        {
            var spec = new OrderSpecifications(orderId, isAdmin: true);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            return order == null ? null : _mapper.Map<OrderDTO>(order);
        }


        public async Task<PaginatedOrdersDTO> GetAllOrdersPaginatedAsync(int pageIndex, int pageSize)
        {
            var spec = new AllOrdersSpecification(pageIndex, pageSize);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);

            var countSpec = new AllOrdersSpecification();
            var totalCount = await _unitOfWork.GetRepository<Order, int>()
                .CountAsync(countSpec);

            return new PaginatedOrdersDTO
            {
                Orders = _mapper.Map<List<OrderDTO>>(orders),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        #endregion

        
        #region Private Methods

        private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
            {
                { OrderStatus.Pending, new[] { OrderStatus.Accepted, OrderStatus.Declined } },
                { OrderStatus.Accepted, new[] { OrderStatus.Paid, OrderStatus.Cancelled } },
                { OrderStatus.Paid, new[] { OrderStatus.Processing } },
                { OrderStatus.Processing, new[] { OrderStatus.Shipped } },
                { OrderStatus.Shipped, new[] { OrderStatus.Delivered } },
                { OrderStatus.Delivered, new[] { OrderStatus.Completed } }
            };

            if (currentStatus == OrderStatus.Completed ||
                currentStatus == OrderStatus.Cancelled ||
                currentStatus == OrderStatus.Declined)
            {
                throw new InvalidOperationException(
                    $"Cannot change status of {currentStatus} order.");
            }

            if (!validTransitions.ContainsKey(currentStatus) ||
                !validTransitions[currentStatus].Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from '{currentStatus}' to '{newStatus}'.");
            }
        }

        #endregion
    }
}