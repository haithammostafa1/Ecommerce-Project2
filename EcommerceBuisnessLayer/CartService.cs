using EcommerceDataLayer;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EcommerceBuisnessLayer
{
    public interface ICartRepository
    {
        Task<clsCartData.CartDto?> GetCartByUserId(int userId);
        Task<int> GetOrCreateCart(int userId);
        Task<int> AddItem(int cartId, int productId, int quantity, decimal unitPrice);
        Task<int> UpdateQuantity(int cartId, int productId, int newQuantity);
        Task<int> RemoveItem(int cartId, int productId);
        Task ClearCart(int cartId);
        Task<clsCartData.CartSummaryDto?> GetSummary(int cartId);
    }

    public class CartRepository : ICartRepository
    {
        public async Task<clsCartData.CartDto?> GetCartByUserId(int userId)
            => await clsCartData.GetCartByUserId(userId);

        public async Task<int> GetOrCreateCart(int userId)
            => await clsCartData.GetOrCreateCart(userId);

        public async Task<int> AddItem(int cartId, int productId, int quantity, decimal unitPrice)
            => await clsCartData.AddItem(cartId, productId, quantity, unitPrice);

        public async Task<int> UpdateQuantity(int cartId, int productId, int newQuantity)
            => await clsCartData.UpdateItemQuantity(cartId, productId, newQuantity);

        public async Task<int> RemoveItem(int cartId, int productId)
            => await clsCartData.RemoveItem(cartId, productId);

        public async Task ClearCart(int cartId)
            => await clsCartData.ClearCart(cartId);

        public async Task<clsCartData.CartSummaryDto?> GetSummary(int cartId)
            => await clsCartData.GetCartSummary(cartId);
    }
    public class CartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository,
                           ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public enum AddItemResult
        {
            Success = 1,
            QuantityUpdated = 2,
            CartNotFound = -1,
            InvalidData = 0,
            Failed = -99
        }

        public enum UpdateItemResult
        {
            Success = 1,
            ItemNotFound = 0,
            Failed = -99
        }

        public enum RemoveItemResult
        {
            Success = 1,
            ItemNotFound = 0,
            Failed = -99
        }

        public async Task<clsCartData.CartDto?> GetUserCart(int userId)
        {
            _logger.LogInformation("Getting cart for UserId {UserId}", userId);

            if (userId <= 0) return null;

            return await _cartRepository.GetCartByUserId(userId);
        }

        public async Task<AddItemResult> AddItem(
            int userId, int productId, int quantity, decimal price)
        {
            if (userId <= 0 || productId <= 0 || quantity <= 0)
                return AddItemResult.InvalidData;

            try
            {
                int cartId = await _cartRepository.GetOrCreateCart(userId);

                int result = await _cartRepository
                    .AddItem(cartId, productId, quantity, price);

                return result switch
                {
                    1 => AddItemResult.Success,
                    2 => AddItemResult.QuantityUpdated,
                    -1 => AddItemResult.CartNotFound,
                    _ => AddItemResult.Failed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return AddItemResult.Failed;
            }
        }

        public async Task<UpdateItemResult> UpdateItem(
            int userId, int productId, int newQuantity)
        {
            try
            {
                int cartId = await _cartRepository.GetOrCreateCart(userId);

                int result = await _cartRepository
                    .UpdateQuantity(cartId, productId, newQuantity);

                return result == 1
                    ? UpdateItemResult.Success
                    : UpdateItemResult.ItemNotFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return UpdateItemResult.Failed;
            }
        }

        // Remove Item
        public async Task<RemoveItemResult> RemoveItem(int userId, int productId)
        {
            try
            {
                int cartId = await _cartRepository.GetOrCreateCart(userId);

                int result = await _cartRepository
                    .RemoveItem(cartId, productId);

                return result == 1
                    ? RemoveItemResult.Success
                    : RemoveItemResult.ItemNotFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                return RemoveItemResult.Failed;
            }
        }

      
        public async Task<bool> ClearCart(int userId)
        {
            try
            {
                int cartId = await _cartRepository.GetOrCreateCart(userId);
                await _cartRepository.ClearCart(cartId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return false;
            }
        }

        // Summary
        public async Task<clsCartData.CartSummaryDto?> GetSummary(int userId)
        {
            int cartId = await _cartRepository.GetOrCreateCart(userId);
            return await _cartRepository.GetSummary(cartId);
        }
    }
}