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
        private readonly IShippingCalculatorService _shippingCalculator;
        private readonly IPaymentService _paymentService;


        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IShippingCalculatorService shippingCalculator, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _shippingCalculator = shippingCalculator;
            _paymentService = paymentService;
        }

        
        
        #region User 

        
        
        public async Task<List<OrderDTO>> GetUserOrdersAsync(string userId)
        {
            var spec = new OrderSpecifications(userId);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);
            var orderDtos = _mapper.Map<List<OrderDTO>>(orders);
            await EnrichOrdersAsync(orderDtos);
            return orderDtos;
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

            var orderDtos = _mapper.Map<List<OrderDTO>>(orders);
            await EnrichOrdersAsync(orderDtos);

            return new PaginatedOrdersDTO
            {
                Orders = orderDtos,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        private async Task EnrichOrdersAsync(List<OrderDTO> orderDtos)
        {
            var customOrderIds = orderDtos.Where(o => o.IsCustom).Select(o => o.Id).ToList();
            if (!customOrderIds.Any()) return;

            var offerRepo = _unitOfWork.GetRepository<Offer, int>();
            var spec = new OffersByOrderIdsSpecification(customOrderIds);
            var offers = await offerRepo.GetAllAsync(spec);
            
            foreach (var orderDto in orderDtos.Where(o => o.IsCustom))
            {
                var offer = offers.FirstOrDefault(off => off.OrderId == orderDto.Id);
                
                if (offer != null && offer.CustomRequest != null)
                {
                    orderDto.Description = offer.CustomRequest.Description;
                    orderDto.ImageUrl = offer.CustomRequest.ImageUrl;
                }
            }
        }

        
        
        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId, string userId)
        {
            var spec = new OrderSpecifications(orderId, userId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            if (order == null) return null;
            
            var orderDto = _mapper.Map<OrderDTO>(order);
            await EnrichOrdersAsync(new List<OrderDTO> { orderDto });
            return orderDto;
        }

        
        
        public async Task<OrderResponseDTO> CreateOrderFromCartAsync(
            string userId, CreateOrderDTO createOrderDTO)
        {
            var cartRepo = _unitOfWork.GetRepository<Cart, int>();
            var cartSpec = new CartWithItemsSpecification(userId);
            var cart = await cartRepo.GetByIdAsync(cartSpec);

            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty!");
            
            await ValidateSellersNotBlockedAsync(cart.CartItems);


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
                    Quantity = cartItem.Quantity,
                    SellerId = cartItem.Product.SellerId
                });
            }

            var categoryIds = cart.CartItems.Select(ci => ci.Product.CategoryId).Distinct();
            var shippingResult = await _shippingCalculator.CalculateShippingAsync(createOrderDTO.City, categoryIds);

            var newOrder = new Order
            {
                UserId = userId,
                SubTotal = totalPrice,
                ShippingCost = shippingResult.ShippingCost,
                TotalPrice = totalPrice + shippingResult.ShippingCost,
                City = createOrderDTO.City,
                ShippingRuleId = shippingResult.ShippingRuleId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = createOrderDTO.ShippingAddress,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            await _unitOfWork.GetRepository<Order, int>().AddAsync(newOrder);


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
        public async Task<OrderResponseDTO> CreateOrderFromOfferAsync(
            string userId, CreateOrderFromOfferDTO dto)
        {
            var offerRepo = _unitOfWork.GetRepository<Offer, int>();
            var offer = await offerRepo.GetByIdAsync(dto.OfferId);

            if (offer == null)
                throw new InvalidOperationException("Offer not found");

            if (offer.Status != OfferStatus.Accepted)
                throw new InvalidOperationException("Offer must be accepted before creating an order");

            if (offer.OrderId != null)
                throw new InvalidOperationException("An order has already been created for this offer");
            
            await ValidateSellerNotBlockedAsync(offer.SellerId);


            // Offers don't have defined category ids, so pass an empty list
            var shippingResult = await _shippingCalculator.CalculateShippingAsync(dto.City ?? "Unknown City", new List<int>());

            var newOrder = new Order
            {
                UserId = userId,
                SubTotal = offer.Price,
                ShippingCost = shippingResult.ShippingCost,
                TotalPrice = offer.Price + shippingResult.ShippingCost,
                City = dto.City ?? "Unknown City",
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = dto.ShippingAddress,
                CreatedAt = DateTime.UtcNow,
                IsCustom = true
            };

            await _unitOfWork.GetRepository<Order, int>().AddAsync(newOrder);
            
            // Link the navigation property - EF will handle the ID assignment during SaveChanges
            offer.Order = newOrder;
            offerRepo.Update(offer);

            await _unitOfWork.SaveChangesAsync();

            return new OrderResponseDTO
            {
                OrderId = newOrder.Id,
                TotalPrice = newOrder.TotalPrice,
                OrderDate = newOrder.OrderDate,
                Status = newOrder.Status.ToString(),
                Message = "Order created successfully from offer!"
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

            var paymentSpec = new PaymentByOrderIdSpecification(orderId);
            var payment = await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(paymentSpec);

            if (payment != null &&
                (payment.Status == PaymentStatus.Pending || payment.Status == PaymentStatus.Processing))
            {
                payment.Status = PaymentStatus.Cancelled;
                _unitOfWork.GetRepository<Payment, int>().Update(payment);
            }

            var payoutSpec = new SellerPayoutByOrderIdSpecification(orderId);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);

            foreach (var payout in payouts.Where(p =>
                         p.Status == PayoutStatus.Pending ||
                         p.Status == PayoutStatus.Processing ||
                         p.Status == PayoutStatus.Failed))
            {
                payout.Status = PayoutStatus.Cancelled;
                _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        
        private async Task ValidateSellerNotBlockedAsync(string sellerId)
        {
            var spec = new SellerProfileByUserIdSpecification(sellerId);
            var sellerProfile = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(spec);

            if (sellerProfile != null && sellerProfile.IsBlocked)
                throw new InvalidOperationException(
                    "Some products in your order are currently unavailable");
        }

        private async Task ValidateSellersNotBlockedAsync(ICollection<CartItem> cartItems)
        {
            var sellerIds = cartItems
                .Where(ci => ci.Product != null)
                .Select(ci => ci.Product.SellerId)
                .Distinct()
                .ToList();

            if (!sellerIds.Any())
                return;

            var spec = new BlockedSellerProfilesByUserIdsSpecification(sellerIds);

            var blockedSellers = await _unitOfWork
                .GetRepository<SellerProfile, int>()
                .GetAllAsync(spec);

            if (blockedSellers.Any())
                throw new InvalidOperationException("Some products in your order are currently unavailable");
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

            if (newStatus == OrderStatus.Paid)
            {
                var paymentSpec = new PaymentByOrderIdSpecification(orderId);
                var payment = await _unitOfWork.GetRepository<Payment, int>()
                    .GetByIdAsync(paymentSpec);

                if (payment == null || payment.Method != PaymentMethod.Card)
                    throw new InvalidOperationException(
                        "Paid status is only valid for card payments.");
            }

            ValidateStatusTransition(order.Status, newStatus);

            if (newStatus == OrderStatus.Processing && order.Status != OrderStatus.Processing)
            {
                var productRepo = _unitOfWork.GetRepository<Product, int>();
                foreach (var item in order.OrderItems ?? new List<OrderItem>())
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

            if (newStatus == OrderStatus.Delivered)
            {
                await _paymentService.ConfirmCashPaymentAfterDeliveryAsync(orderId);
            }

            return true;
        }

        public async Task<List<OrderDTO>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var spec = new OrderByStatusSpecification(status);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);
            var orderDtos = _mapper.Map<List<OrderDTO>>(orders);
            await EnrichOrdersAsync(orderDtos);
            return orderDtos;
        }


        public async Task<OrderDTO?> GetOrderByIdForAdminAsync(int orderId)
        {
            var spec = new OrderSpecifications(orderId, isAdmin: true);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            if (order == null) return null;
            
            var orderDto = _mapper.Map<OrderDTO>(order);
            await EnrichOrdersAsync(new List<OrderDTO> { orderDto });
            return orderDto;
        }


        public async Task<PaginatedOrdersDTO> GetAllOrdersPaginatedAsync(int pageIndex, int pageSize)
        {
            var spec = new AllOrdersSpecification(pageIndex, pageSize);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);

            var countSpec = new AllOrdersSpecification();
            var totalCount = await _unitOfWork.GetRepository<Order, int>()
                .CountAsync(countSpec);

            var orderDtos = _mapper.Map<List<OrderDTO>>(orders);
            await EnrichOrdersAsync(orderDtos);

            return new PaginatedOrdersDTO
            {
                Orders = orderDtos,
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
                { OrderStatus.Accepted, new[] { OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Cancelled } },
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

        #region Seller

        public async Task<List<OrderDTO>> GetOrdersForSellerAsync(string sellerId)
        {
            var spec = new SellerOrdersSpecification(sellerId);
            var orders = await _unitOfWork.GetRepository<Order, int>()
                .GetAllAsync(spec);

            var orderDtos = _mapper.Map<List<OrderDTO>>(orders);
            await EnrichOrdersAsync(orderDtos);

            return orderDtos;
        }

        public async Task<OrderDTO?> GetOrderByIdForSellerAsync(int orderId, string sellerId)
        {
            var spec = new SellerOrderByIdSpecification(orderId, sellerId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            if (order == null) return null;

            var orderDto = _mapper.Map<OrderDTO>(order);
            await EnrichOrdersAsync(new List<OrderDTO> { orderDto });
            return orderDto;
        }

        #endregion
    }
}