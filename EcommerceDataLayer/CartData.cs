using EcommerceDataLayer.ConnectionSeeting;
using Microsoft.Data.SqlClient;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EcommerceDataLayer
{
    public class clsCartData
    {
      
        public class CartItemDto
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal SubTotal { get; set; }
            public DateTime AddedAt { get; set; }
        }

        public class CartDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public List<CartItemDto> Items { get; set; } = new();
        }

        public class CartSummaryDto
        {
            public int CartId { get; set; }
            public int TotalItems { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private static CartItemDto MapToCartItemDto(SqlDataReader reader)
        {
            return new CartItemDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("ItemId")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                AddedAt = reader.GetDateTime(reader.GetOrdinal("AddedAt"))
            };
        }

      
        public static async Task<CartDto?> GetCartByUserId(int userId)
        {
            Log.Information("DAL: Getting cart for user {UserId}", userId);

            CartDto? cart = null;

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_GetCartByUserId", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (cart == null)
                    {
                        cart = new CartDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("ItemId")))
                        cart.Items.Add(MapToCartItemDto(reader));
                }

                return cart;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error getting cart for user {UserId}", userId);
                throw;
            }
        }

       
        public static async Task<int> GetOrCreateCart(int userId)
        {
            Log.Information("DAL: GetOrCreate cart for user {UserId}", userId);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_GetOrCreateCart", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                var output = cmd.Parameters.Add("@CartId", SqlDbType.Int);
                output.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return (int)output.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error in GetOrCreateCart");
                throw;
            }
        }

        
        public static async Task<int> AddItem(
            int cartId, int productId, int quantity, decimal unitPrice)
        {
            Log.Information("DAL: Adding product {ProductId} to cart {CartId}",
                productId, cartId);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_AddItemToCart", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CartId", SqlDbType.Int).Value = cartId;
                cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = unitPrice;

                var resultParam = cmd.Parameters.Add("@ResultCode", SqlDbType.Int);
                resultParam.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return (int)resultParam.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error adding item to cart");
                throw;
            }
        }

        public static async Task<int> UpdateItemQuantity(
            int cartId, int productId, int newQuantity)
        {
            Log.Information("DAL: Updating product {ProductId} in cart {CartId}",
                productId, cartId);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_UpdateCartItemQuantity", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CartId", SqlDbType.Int).Value = cartId;
                cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@NewQuantity", SqlDbType.Int).Value = newQuantity;

                var resultParam = cmd.Parameters.Add("@ResultCode", SqlDbType.Int);
                resultParam.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return (int)resultParam.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error updating cart item");
                throw;
            }
        }

        // ================================
        // Remove Item
        // ================================

        public static async Task<int> RemoveItem(int cartId, int productId)
        {
            Log.Information("DAL: Removing product {ProductId} from cart {CartId}",
                productId, cartId);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_RemoveItemFromCart", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CartId", SqlDbType.Int).Value = cartId;
                cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

                var resultParam = cmd.Parameters.Add("@ResultCode", SqlDbType.Int);
                resultParam.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return (int)resultParam.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error removing cart item");
                throw;
            }
        }

        // ================================
        // Clear Cart
        // ================================

        public static async Task ClearCart(int cartId)
        {
            Log.Information("DAL: Clearing cart {CartId}", cartId);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_ClearCart", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CartId", SqlDbType.Int).Value = cartId;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error clearing cart");
                throw;
            }
        }

        // ================================
        // Get Cart Summary
        // ================================

        public static async Task<CartSummaryDto?> GetCartSummary(int cartId)
        {
            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_GetCartSummary", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CartId", SqlDbType.Int).Value = cartId;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new CartSummaryDto
                    {
                        CartId = reader.GetInt32(reader.GetOrdinal("CartId")),
                        TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems")),
                        TotalQuantity = reader.GetInt32(reader.GetOrdinal("TotalQuantity")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error getting cart summary");
                throw;
            }
        }
    }
}