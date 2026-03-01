using EcommerceDataLayer.ConnectionSeeting;
using EcommerceDataLayer.Dto;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceDataLayer
{
   



    public class OrderData
    {
      


        static public async Task<int> CreatOrder(int @UserId)
        {
     
           using SqlConnection conn = new(clsAccessSettings.ConnectionString);
           using SqlCommand cmd = new("sp_CreateOrder", conn);
         
           cmd.CommandType = CommandType.StoredProcedure;
           cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = @UserId;
           var Output = cmd.Parameters.Add("@NewId", SqlDbType.Int);
           Output.Direction = ParameterDirection.Output;
           await conn.OpenAsync();
           await cmd.ExecuteNonQueryAsync();
           return (int)Output.Value;

        }
        static public async Task<int> AddOrderItem(int OrderId,int ProductId,
            int Quantity,decimal Price)
        {
            using SqlConnection conn = new(clsAccessSettings.ConnectionString);
            using SqlCommand cmd = new("SP_AddOrderItem", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Orderid", SqlDbType.Int).Value = OrderId;
            cmd.Parameters.Add("@productid", SqlDbType.Int).Value = ProductId;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = Quantity;
            cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = Price;

            var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return returnParam.Value != DBNull.Value ? Convert.ToInt32(returnParam.Value) : 0;

        }
        public static async Task<int> UpdateOrderTotal(
               int orderId, decimal total)
        {
            using SqlConnection conn = new(clsAccessSettings.ConnectionString);
            using SqlCommand cmd = new("sp_UpdateOrderTotal", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@OrederId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = total;

            var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return returnParam.Value != DBNull.Value ? Convert.ToInt32(returnParam.Value) : 0;
           
        }
        public static async Task<int> DecreaseProductStock(int productid,int Quantity)
        {

            using SqlConnection conn = new(clsAccessSettings.ConnectionString);
            using SqlCommand cmd = new("sp_DecreaseProductStock", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@productid", SqlDbType.Int).Value = productid;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = Quantity;

            var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return returnParam.Value != DBNull.Value ? Convert.ToInt32(returnParam.Value) : 0;

        }
      
        public static async Task<int> CancelOrder(int OrderId)
        {
         
           using SqlConnection conn = new(clsAccessSettings.ConnectionString);
           using SqlCommand cmd = new("sp_CancelOrder", conn);

           cmd.CommandType = CommandType.StoredProcedure;
           cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = OrderId;

           var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
           returnParam.Direction = ParameterDirection.ReturnValue;

           await conn.OpenAsync();
           await cmd.ExecuteNonQueryAsync();
           return returnParam.Value != DBNull.Value ? Convert.ToInt32(returnParam.Value) : 0;

    
        }
        public static async Task<List<OrderItemStockDto>> GetOrderItemsForCancel(int OrderId)
        {
            List<OrderItemStockDto> items = new();

            using SqlConnection conn = new(clsAccessSettings.ConnectionString);
            using SqlCommand cmd = new("sp_GetOrderItemForCancel", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = OrderId;
            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new OrderItemStockDto
                {
                    ProductId = reader.GetInt32(0),
                    Quantity = reader.GetInt32(1)
                });
            }

            return items;
        }

        public static async Task<int> IncreaseProduct(int ProductId, int Quantity)
        {
            using SqlConnection conn = new(clsAccessSettings.ConnectionString);
            using SqlCommand cmd = new("SP_IncreaseProductStock", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = ProductId;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = Quantity;

            var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return returnParam.Value != DBNull.Value ? Convert.ToInt32(returnParam.Value) : 0;
        }
    }
}

