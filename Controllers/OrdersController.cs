using EcommerceBuisnessLayer;
using EcommerceBuisnessLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace EcommrceApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {

        private readonly OrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        [Authorize(Roles = "User,Admin")]
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDto request)
        {
            _logger.LogInformation("HTTP PlaceOrder request received.");

            // 3. استدعاء الدالة
            var result = await _orderService.PlaceOrder(request);

            // 4. تصحيح طريقة كتابة الـ Enum (اسم الكلاس . اسم الـ Enum)
            return result switch
            {
                OrderService.PlaceOrderResult.Success =>
                    Ok(new { message = "Order placed successfully" }),

                OrderService.PlaceOrderResult.InvalidData =>
                    BadRequest(new { message = "Invalid order data" }),

                OrderService.PlaceOrderResult.ProductNotFound =>
                    NotFound(new { message = "One or more products not found" }),

                OrderService.PlaceOrderResult.InsufficientStock =>
                    BadRequest(new { message = "Insufficient stock for one or more products" }),

                OrderService.PlaceOrderResult.Failed =>
                    StatusCode(500, new { message = "Internal server error processing the order" }),

                _ =>
                    StatusCode(500, new { message = "Unexpected error occurred" })
            };
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            _logger.LogInformation("HTTP CancelOrder request received for Order ID: {OrderId}", orderId);
            var result = await _orderService.CancelOrderAsync(orderId);

            return result switch
            {
                OrderService.CancelOrderResult.Success =>
                    Ok(new { message = "Order canceled successfully" }),

                OrderService.CancelOrderResult.OrderNotFound =>
                    NotFound(new { message = "Order not found" }),

                OrderService.CancelOrderResult.OrderAlreadyCanceled =>
                    BadRequest(new { message = "Order is already canceled" }),

                OrderService.CancelOrderResult.OrderCannotBeCanceled =>
                    BadRequest(new { message = "Order cannot be canceled (shipped or too late)" }),

                _ => StatusCode(500, new { message = "An error occurred while canceling order" })
            };
        }
    }
}