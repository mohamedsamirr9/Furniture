using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Furniture.shared.Dtos.Cart;

namespace Furniture.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; 
        private readonly IRecommendationService _recommendationService;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper, IRecommendationService recommendationService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _recommendationService = recommendationService;
            
        }

        #region GET CART

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cart = await GetCartWithItemsAsync(userId);

            if (cart == null)
                cart = await CreateNewCartAsync(userId);

            return _mapper.Map<CartDto>(cart);       
        }

        #endregion


        #region ADD TO CART

        public async Task<CartDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            if (dto.Quantity <= 0)
                throw new InvalidOperationException("Quantity Must Be Over 0");

            var product = await _unitOfWork.GetRepository<Product, int>()
                .GetByIdAsync(dto.ProductId);

            if (product == null)
                throw new KeyNotFoundException("Product Not Found");

            if (product.StockQuantity < dto.Quantity)
                throw new InvalidOperationException(
                    $"This Quantity Is Not Available . The Available Is: {product.StockQuantity}");

            var cart = await GetCartWithItemsAsync(userId);

            if (cart == null)
                cart = await CreateNewCartAsync(userId);

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;

                if (product.StockQuantity < newQuantity)
                    throw new InvalidOperationException(
                        $"This Quantity Is Not Available . The Available Is: {product.StockQuantity}، " +
                        $"In The Cart: {existingItem.Quantity}");

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = product.Price;

                _unitOfWork.GetRepository<CartItem, int>().Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price
                };

                await _unitOfWork.GetRepository<CartItem, int>().AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            
            await _recommendationService.UpdateUserEmbeddingAsync(
                userId, dto.ProductId, "cart");


            return await GetCartAsync(userId);
        }


        #endregion

        #region UPDATE CART ITEM

        public async Task<CartDto> UpdateCartItemAsync(
            string userId, int productId, UpdateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                throw new InvalidOperationException("Quantity Must Be Over 0");

            var product = await _unitOfWork.GetRepository<Product, int>()
                .GetByIdAsync(productId);

            if (product == null)
                throw new KeyNotFoundException("Product Not Found !");

            if (product.StockQuantity < dto.Quantity)
                throw new InvalidOperationException(
                    $"This Quantity Is Not Available . The Available Is: {product.StockQuantity}");

            var cart = await GetCartWithItemsAsync(userId);

            if (cart == null)
                throw new InvalidOperationException("Cart Is Empty !");

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
                throw new KeyNotFoundException("Product Is Not In The Cart !");

            cartItem.Quantity = dto.Quantity;
            cartItem.UnitPrice = product.Price;

            _unitOfWork.GetRepository<CartItem, int>().Update(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return await GetCartAsync(userId);
        }


        #endregion

        #region REMOVE FROM CART

        public async Task<CartDto> RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await GetCartWithItemsAsync(userId);

            if (cart == null)
                throw new InvalidOperationException("Cart Is Empty !");

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
                throw new KeyNotFoundException("Product Is Not In The Cart !");

            _unitOfWork.GetRepository<CartItem, int>().Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        #endregion


        #region CLEAR CART

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartWithItemsAsync(userId);

            if (cart == null || !cart.CartItems.Any())
                return;

            _unitOfWork.GetRepository<CartItem, int>()
                .RemoveRange(cart.CartItems);

            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region PRIVATE HELPERS

        private async Task<Cart?> GetCartWithItemsAsync(string userId)
        {
            var spec = new CartWithItemsSpecification(userId);
            return await _unitOfWork.GetRepository<Cart, int>()
                .GetByIdAsync(spec);
        }

        private async Task<Cart> CreateNewCartAsync(string userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<Cart, int>().AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            return cart;
        }

        #endregion

    }
}