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
        private readonly IAuthorizationService _authorizationService;

        public CartController(CartService cartService, IAuthorizationService authorizationService)
        {
            _cartService = cartService;
            _authorizationService = authorizationService;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart(int userId)
        {

            var authResult = await _authorizationService.AuthorizeAsync(
          User,
            userId,
             "UserOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            var cart = await _cartService.GetUserCart(userId);
            if (cart == null) return NotFound("Cart not found");
            return Ok(cart);
        }

        [HttpPost("{userId}/add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem(
            int userId, int productId, int quantity, decimal price)
        {

            var authResult = await _authorizationService.AuthorizeAsync(
          User,
            userId,
             "UserOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403
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
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(
            int userId, int productId, int quantity)
        {

            var authResult = await _authorizationService.AuthorizeAsync(
          User,
            userId,
             "UserOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            var result = await _cartService
                .UpdateItem(userId, productId, quantity);

            return result switch
            {
                CartService.UpdateItemResult.Success => Ok("Updated"),
                CartService.UpdateItemResult.ItemNotFound => NotFound(),
                _ => StatusCode(500)
            };
        }
       
        [HttpDelete("{userId}/remove/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItem(int userId, int productId)
        {

            var authResult = await _authorizationService.AuthorizeAsync(
          User,
            userId,
             "UserOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            var result = await _cartService.RemoveItem(userId, productId);

            return result switch
            {
                CartService.RemoveItemResult.Success => Ok("Removed"),
                CartService.RemoveItemResult.ItemNotFound => NotFound(),
                _ => StatusCode(500)
            };
        }
        
        [HttpDelete("{userId}/clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Clear(int userId)
        {

            var authResult = await _authorizationService.AuthorizeAsync(
          User,
            userId,
             "UserOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            var success = await _cartService.ClearCart(userId);
            return success ? Ok("Cart Cleared") : StatusCode(500);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpGet("{userId}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary(int userId)
        {

            var authResult = await _authorizationService.AuthorizeAsync(
              User,
            userId,
             "UserOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            var summary = await _cartService.GetSummary(userId);
            return Ok(summary);
        }
    }





}

