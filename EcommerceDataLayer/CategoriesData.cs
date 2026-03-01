using EcommerceDataLayer.Helpers;
using Microsoft.Data.SqlClient;
using System;

using System.ComponentModel.DataAnnotations;
using System.Data;


using Serilog;
using EcommerceDataLayer.ConnectionSeeting;
namespace EcommerceDataLayer
{




    public class CategoryDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "This Is Requied")]
        [MaxLength(100, ErrorMessage = " Max Can Not  Be Bigger Than 100 ")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "This Is Requied")]
        [MaxLength(150, ErrorMessage = " Max Can Not  Be Bigger Than 150 ")]

        public string Slug { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "This Number Must Be Positive")] // لضمان الأرقام الموجبة
        public int? ParentId { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime UpdatedAt { get; set; }


    }

    public class CategoriesData
    {
        private static CategoryDto MapToCategoryDto(SqlDataReader reader)
        {
            return new CategoryDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Slug = reader.GetString(reader.GetOrdinal("Slug")),

                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),

                ParentId = reader.IsDBNull(reader.GetOrdinal("ParentId"))
                           ? null
                           : reader.GetInt32(reader.GetOrdinal("ParentId")),

                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                              ? null
                              : reader.GetString(reader.GetOrdinal("Description")),

                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl"))
                           ? null
                           : reader.GetString(reader.GetOrdinal("ImageUrl")),

                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                IsDeleted=reader.IsDBNull(reader.GetOrdinal("IsDeleted"))
                ? null
                :reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
               

            };
        }

        public static async Task<CategoryDto?> GetCategoryById(int Id)
        {
            Log.Information("DAL:Try To Find Category By Id {ID} ",Id);
            CategoryDto? category = null;
            //Id,Name,Slug,parentId,Description,ImageUrl 
            try
            {
                using SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString);
                using (SqlCommand Command = new SqlCommand("sp_GetCategoryById", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                    await Connection.OpenAsync();
                    using (var reader = await Command.ExecuteReaderAsync())
                    {

                        if (await reader.ReadAsync())
                        {                          
                            category = (MapToCategoryDto(reader));
                            Log.Information("DAL:Cateogry With {Id} is Exist ", Id);
                        }

                    }
                }         
                return category;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured When Finding Category With {Id} ", Id);
                throw;
            }
        }
        public static async Task<int> AddNewCategory(CategoryDto dto)
        {
            Log.Information("DAL:Attempting To Add New Category: {Name}", dto.Name);
            try
            {
                using SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString);

                using (SqlCommand Command = new SqlCommand("sp_AddNewCategory", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = dto.Name;
                    Command.Parameters.Add("@Slug", SqlDbType.VarChar, 150).Value = dto.Slug;
                    Command.Parameters.Add("@ParentId", SqlDbType.Int).Value = (object?)dto.ParentId ?? DBNull.Value;
                    Command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = dto.IsActive;
                    Command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)dto.Description ?? DBNull.Value;
                    Command.Parameters.Add("@ImageUrl", SqlDbType.VarChar, -1).Value = (object?)dto.ImageUrl ?? DBNull.Value;
                    Command.Parameters.Add("@DisplayOrder", SqlDbType.Int).Value = dto.DisplayOrder;

                    await Connection.OpenAsync();

                    object? result = await Command.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int InsertedId))
                    {
                        Log.Information(" DAL :Category added successfully. New ID: {Id}, Name: {Name}", InsertedId, dto.Name);
                        return InsertedId;
                    }
                    else
                    {
                        Log.Warning("DAL: Query executed but failed to retrieve ID for category: {Name}", dto.Name);
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Failed to add category: {Name}", dto.Name);
                throw;
            }
        }
        public static async Task<bool> IsCategoryExist(int id)
        {
            Log.Information("DAL : Attempting To Category If Exist With {id}", id);
            try
            {
                using (SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand Command = new SqlCommand("sp_CheckCategoryIdExists", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    await Connection.OpenAsync();
                    var result = await Command.ExecuteScalarAsync();

                    if (result != null)
                    {
                        Log.Information("DAL :Category Is  Exist With {id}", id);
                        return true;
                    }
                    else
                    {
                        Log.Warning("DAL :Category Is Not Exist With {id}", id);
                    }
                }
            }
            
            catch (Exception ex)
            {
                Log.Error(ex, "Error Can Not Found Category");
               throw;
            }
            return false;

        }
        public static async Task<bool> CategoryExists(string CategoryName, int? id = null)
        {
            Log.Information("DAL: Checking availability of category name: '{Name}' (ID context: {Id})", CategoryName, id);

            try
            {
                using SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString);
                using (SqlCommand Command = new SqlCommand("sp_CheckCategoryNameExists", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;

                    Command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object?)CategoryName ?? DBNull.Value;
                    Command.Parameters.Add("@ExcludeId", SqlDbType.Int).Value = (object?)id ?? -1;

                    await Connection.OpenAsync();
                    object? result = await Command.ExecuteScalarAsync();

                    if (result != null && bool.TryParse(result.ToString(), out bool Exist))
                    {
                        if (Exist) Log.Information("DAL: Name '{Name}' is already taken.", CategoryName);
                        else Log.Information("DAL: Name '{Name}' is available.", CategoryName);

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
                Log.Error(ex, "DAL: Database failed while checking name '{Name}'", CategoryName);
                throw;
            }
        }
        public static async Task<int> UpdateCategory(CategoryDto dto)
        {
            Log.Information("DAL: Updating Category ID: {Id}", dto.Id);

            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_UpdateCategory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@Id", SqlDbType.Int).Value = dto.Id;
                    command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = dto.Name;
                    command.Parameters.Add("@Slug", SqlDbType.VarChar, 150).Value = dto.Slug;
                    command.Parameters.Add("@ParentId", SqlDbType.Int).Value = (object?)dto.ParentId ?? DBNull.Value;
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = dto.IsActive;
                    command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)dto.Description ?? DBNull.Value;
                    command.Parameters.Add("@ImageUrl", SqlDbType.VarChar, -1).Value = (object?)dto.ImageUrl ?? DBNull.Value;
                    command.Parameters.Add("@DisplayOrder", SqlDbType.Int).Value = dto.DisplayOrder;

                    var returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

              
                    int result = (int)returnParameter.Value;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error updating category ID {Id}", dto.Id);
                throw;
            }
        }
        public static async Task<int> DeleteCategory(int id)
        {
            Log.Information("DAL: Attempting to soft delete category ID: {Id}", id);

            try
            {
                using (SqlConnection connection = new SqlConnection(clsAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("sp_DeleteCategory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

             
                    var returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    int result = (int)returnParameter.Value;

                    Log.Information("DAL: Delete result for ID {Id} is: {Result}", id, result);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error deleting category ID {Id}", id);
                throw; // بنرمي الخطأ عشان الكنترولر يهندله بـ 500
            }
        }
        public static async Task<PagedList<CategoryDto>> GetCategoriesPagedAsync(PaginationParams paginationParams)
        {
            Log.Information("DAL: Starting fetching categories. Page: {Page}, Size: {Size}", paginationParams.PageNumber, paginationParams.PageSize);
            var CategoryList = new List<CategoryDto>();
            int TotalCount = 0;
            try
            {
                using SqlConnection Connection = new SqlConnection(clsAccessSettings.ConnectionString);
                using (SqlCommand Command = new SqlCommand("sp_GetCategoriesPaged", Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = paginationParams.PageNumber;
                    Command.Parameters.Add("@PageSize", SqlDbType.Int).Value = paginationParams.PageSize;
                    var TotalCountParam = new SqlParameter
                    {
                        ParameterName = "@TotalCount",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output,
                    };
                    Command.Parameters.Add(TotalCountParam);
                    await Connection.OpenAsync();
                    using (SqlDataReader reader = await Command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            CategoryList.Add(MapToCategoryDto(reader));
                        }
                    }
                    if (TotalCountParam.Value is int tc)
                        TotalCount = tc;
                    return new PagedList<CategoryDto>(CategoryList, TotalCount,
                        paginationParams.PageNumber, paginationParams.PageSize);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Database Error in GetCategoriesPagedAsync");
                throw;

            }
        }

    }

}


            
        



    

