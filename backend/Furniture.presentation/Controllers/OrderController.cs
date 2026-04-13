using Furniture.Domain.Models.Enum;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        
        

        #region User 

        private readonly string userId = "seller-1";        
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        
        
        [HttpGet("paginated")]
        public async Task<IActionResult> GetMyOrdersPaginated(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _orderService.GetUserOrdersPaginatedAsync(userId, pageIndex, pageSize);
            return Ok(result);
        }

        
        
        
        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(orderId, userId);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        
        
        
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO createOrderDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _orderService.CreateOrderFromCartAsync(userId, createOrderDTO);
                return CreatedAtAction(nameof(GetOrderById), new { orderId = result.OrderId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("from-offer")]
        public async Task<IActionResult> CreateOrderFromOffer([FromBody] CreateOrderFromOfferDTO createOrderFromOfferDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _orderService.CreateOrderFromOfferAsync(userId, createOrderFromOfferDTO);
                return CreatedAtAction(nameof(GetOrderById), new { orderId = result.OrderId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        
        [HttpDelete("{orderId:int}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _orderService.CancelOrderAsync(orderId, userId);
                if (!result)
                    return NotFound(new { message = "Order not found" });

                return Ok(new { message = "Order cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Admin 

       
        
        [HttpGet("admin/all")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetAllOrdersPaginatedAsync(pageIndex, pageSize);
            return Ok(result);
        }

       
        
        [HttpGet("admin/status/{status}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                return BadRequest(new { message = "Invalid status. Valid values: Pending, Accepted, Paid, Processing, Shipped, Delivered, Completed, Cancelled, Declined" });

            var orders = await _orderService.GetOrdersByStatusAsync(orderStatus);
            return Ok(orders);
        }

       
        
        [HttpGet("admin/{orderId:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderByIdForAdmin(int orderId)
        {
            var order = await _orderService.GetOrderByIdForAdminAsync(orderId);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        
        
        [HttpPut("admin/{orderId:int}/status")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            [FromBody] UpdateOrderStatusDTO updateDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Enum.TryParse<OrderStatus>(updateDTO.Status, true, out var newStatus))
                return BadRequest(new { message = "Invalid status" });

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, adminId!);
                if (!result)
                    return NotFound(new { message = "Order not found" });

                return Ok(new { message = $"Order status updated to {newStatus}" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}