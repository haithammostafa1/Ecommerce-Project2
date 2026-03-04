using EcommerceBuisnessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace EcommrceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {


        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart(int userId)
        {
            var CartAuthCalimId = User.FindFirstValue(ClaimTypes.NameIdentifier);




            if (!int.TryParse(CartAuthCalimId, out int cartauthCalimid))
            {
                return Unauthorized(new { message = "Invalid user ID" });
            }
            var UserRole = User.FindFirstValue(ClaimTypes.Role);
            bool IsAdmin = UserRole == "Admin";

            if (!IsAdmin && cartauthCalimid != userId)
            {
                return Forbid();
            }


            var cart = await _cartService.GetUserCart(userId);
            if (cart == null) return NotFound("Cart not found");
            return Ok(cart);
        }

        [HttpPost("{userId}/add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> AddItem(
            int userId, int productId, int quantity, decimal price)
        {
            var result = await _cartService
                .AddItem(userId, productId, quantity, price);

            return result switch
            {
                CartService.AddItemResult.Success => Ok("Item Added"),
                CartService.AddItemResult.QuantityUpdated => Ok("Quantity Updated"),
                CartService.AddItemResult.CartNotFound => NotFound(),
                CartService.AddItemResult.InvalidData => BadRequest(),
                _ => StatusCode(500)
            };
        }
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{userId}/update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(
            int userId, int productId, int quantity)
        {
            var result = await _cartService
                .UpdateItem(userId, productId, quantity);

            return result switch
            {
                CartService.UpdateItemResult.Success => Ok("Updated"),
                CartService.UpdateItemResult.ItemNotFound => NotFound(),
                _ => StatusCode(500)
            };
        }
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{userId}/remove/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItem(int userId, int productId)
        {
            var result = await _cartService.RemoveItem(userId, productId);

            return result switch
            {
                CartService.RemoveItemResult.Success => Ok("Removed"),
                CartService.RemoveItemResult.ItemNotFound => NotFound(),
                _ => StatusCode(500)
            };
        }
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{userId}/clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Clear(int userId)
        {
            var success = await _cartService.ClearCart(userId);
            return success ? Ok("Cart Cleared") : StatusCode(500);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpGet("{userId}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary(int userId)
        {
            var summary = await _cartService.GetSummary(userId);
            return Ok(summary);
        }
    }





}

