using EcommerceDataLayer.ConnectionSeeting;
using EcommerceDataLayer.Helpers;
using Microsoft.Data.SqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceDataLayer
{
    public class ProductDTO
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Product Name Is Required")]
        [MaxLength(100, ErrorMessage = "Max length is 200 characters")]
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }
        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }

    }

    public class ProductsData
    {
        public static async Task<int> DeleteProduct(int id)
        {
            Log.Information("DAL: Attempting to soft delete Product ID: {Id}", id);

            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_DeleteProductById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                    var returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    // قراءة النتيجة (1 أو 0)
                    int result = (int)returnParameter.Value;

                    Log.Information("DAL: Delete result for Product ID {Id} is: {Result}", id, result);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error deleting Product ID {Id}", id);
                throw;
            }
        }



        public static ProductDTO MapToProductDto(SqlDataReader reader)
        {
            return new ProductDTO
            {
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Slug = reader.GetString(reader.GetOrdinal("Slug")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null :
                reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null :
                reader.GetString(reader.GetOrdinal("ImageUrl")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public static async Task<bool> IsProductExistById(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_IsProductExist", connection))
                {
                    command.CommandType= CommandType.StoredProcedure;
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    await connection.OpenAsync();

                    var Result = await command.ExecuteScalarAsync();
                    if (Result != null)
                    {
                        Log.Information("DAL :Product Is  Exist With {id}", id);
                        return true;
                    }
                    else
                    {
                        Log.Warning("DAL :Product Is Not Exist With {id}", id);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Can Not Found Product");
                throw;

            }

            return false;


        }
        public static async Task<PagedList<ProductDTO>> GetAllProductsPaged(PaginationParams pagination)
        {
            var ProductsList = new List<ProductDTO>();
            int TotalCount = 0;
            Log.Information("DAL: Fetching products page {Page}, size {Size}", pagination.PageNumber, pagination.PageSize);
            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_GetAllProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pagination.PageNumber;
                    command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pagination.PageSize;

                    var TotalCountParam = new SqlParameter
                    {
                        ParameterName = "@TotalCount",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(TotalCountParam);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ProductsList.Add(MapToProductDto(reader));

                        }
                    }
                    if (TotalCountParam.Value is int tc)
                        TotalCount = tc;
                    Log.Information("DAL: Successfully retrieved {Count} products.", ProductsList.Count);
                    return new PagedList<ProductDTO>(ProductsList, TotalCount,
                        pagination.PageNumber, pagination.PageSize);

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Database Error in GetProductsPagedAsync");

                throw;

            }
        }
        public static async Task<int> UpdateProducts(ProductDTO dto)
        {
            Log.Information("DAL: Updating Product ID: {Id}", dto.ID);

            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_UpdateProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@ID", SqlDbType.Int).Value = dto.ID;

                    command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = dto.Name;
                    command.Parameters.Add("@Slug", SqlDbType.VarChar, 150).Value = dto.Slug;
                    command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (object?)dto.Description ?? DBNull.Value;
                    command.Parameters.Add("@Price", SqlDbType.Decimal).Value = dto.Price;
                    command.Parameters.Add("@Stock", SqlDbType.Int).Value = dto.Stock;
                    command.Parameters.Add("@ImageUrl", SqlDbType.NVarChar).Value = (object?)dto.ImageUrl ?? DBNull.Value;
                    command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = dto.CategoryId;
                    command.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = dto.CategoryName;

                    var ReternValue = command.Parameters.Add("@ReternValue", SqlDbType.Int);

                    ReternValue.Direction = ParameterDirection.ReturnValue;
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    int result = (int)ReternValue.Value;

                    return result;


                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error updating category ID {Id}", dto.ID);
                throw;
            }
        }
        public static async Task<ProductDTO?> GetProductById(int productId)
        {
            ProductDTO? product = null;
            Log.Information("DAL:Try To Find  product By Id {ID} ", productId);
            try
            {
                using SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString);
                using (SqlCommand Command = new SqlCommand("sp_GetProductById", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Parameters.Add("@Id", SqlDbType.Int).Value = productId;
                    await Connection.OpenAsync();
                    using (var reader = await Command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {

                            product = MapToProductDto(reader);
                            Log.Information("DAL:product With {Id} is Exist ", productId);
                        }
                    }
                }
                return product;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured When Finding product With {Id} ", productId);
                throw;
            }
        }
        public static async Task<bool> CheckProductNameExists(string productName, int? ExcludeId = null)
        {
            Log.Information("DAL: Checking availability of product name: '{Name}'", productName);
            try
            {
                using SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString);
                using (SqlCommand Command = new SqlCommand("sp_CheckProductNameExists", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = productName;
                    Command.Parameters.Add("ExcludeId", SqlDbType.Int).Value = (object?)ExcludeId ?? -1;
                    await Connection.OpenAsync();

                    object? result = await Command.ExecuteScalarAsync();
                    if (result != null && bool.TryParse(result.ToString(), out bool Exist))
                    {
                        if (Exist) Log.Information("DAL: Name '{Name}' is already taken.", productName);
                        else Log.Information("DAL: Name '{Name}' is available.", productName);
                        return Exist;
                    }
                    else
                    {
                        Log.Warning("DAL: CheckName query returned unexpected result. Assuming False.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Database failed while checking name '{Name}'", productName);
                throw;
            } 
        }
        public static async Task<int> AddNewProduct(ProductDTO newproduct)
        {
            Log.Information("DAL:Attempting To AddNew Product{Name} ", newproduct.Name);
            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_AddNewProduct", connection))
                {
                    command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = newproduct.Name;
                    command.Parameters.Add("@Slug", SqlDbType.VarChar, 150).Value = newproduct.Slug;
                    command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (object?)newproduct.Description ?? DBNull.Value;
                    command.Parameters.Add("@Price", SqlDbType.Decimal).Value = newproduct.Price;
                    command.Parameters.Add("@Stock", SqlDbType.Int).Value = newproduct.Stock;
                    command.Parameters.Add("@ImageUrl", SqlDbType.VarChar, -1).Value = (object?)newproduct.ImageUrl ?? DBNull.Value;
                    command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = newproduct.CategoryId;

                    var OutIdPatameter = new SqlParameter
                    {
                        ParameterName = "@NewId",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output,

                    };
                    command.Parameters.Add(OutIdPatameter);
                    await connection.OpenAsync();

                    await command.ExecuteNonQueryAsync();
                    int NewId = (int)OutIdPatameter.Value;
                    if (NewId == -1)
                    {
                        Log.Warning("DAL: Failed to add product. Category ID {CatId} does not exist.", newproduct.CategoryId);
                    }
                    else
                    {
                        Log.Information("DAL: Product added successfully with ID: {Id}", NewId);
                    }
                    return NewId;
                }
            }
            catch
            (Exception ex)
            {
                Log.Error(ex, "DAL: Error adding new product '{Name}'", newproduct.Name);
                throw;
            }

        }
        public static async Task<int> IncreaseProduct(int ProductId, int Quantity)
        {
            using SqlConnection con = new(clsAccessSettings.ConnectionString);
            using SqlCommand command = new SqlCommand("SP_IncreaseProductStock", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = ProductId;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = Quantity;

            var ReturnValue = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
            ReturnValue.Direction = ParameterDirection.ReturnValue;

            await con.OpenAsync();
            await command.ExecuteScalarAsync();
            return (int)ReturnValue.Value;



        }
    }
}
