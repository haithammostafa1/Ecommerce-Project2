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
    public class clsUserData
    {
        public class UserResponseDto
        {
            [Required]
            public int Id { get; set; }

            [Required]
            [MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [MaxLength(150)]
            public string Email { get; set; } = string.Empty;
            [Required]
            [MaxLength(100)]
            public string Role { get; set; } = string.Empty;
            [Phone]
            [MaxLength(20)]
            public string? Phone { get; set; }

            public string? Address { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class UseCreateDto
        {
            [Required]
            public int Id { get; set; }

            [Required]
            [MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [MaxLength(150)]
            public string Email { get; set; } = string.Empty;
            [Required]
            [MaxLength(255)]
            public string PasswordHash { get; set; } = string.Empty;
            [Required]
            [MaxLength(100)]
            public string Role { get; set; } = string.Empty;
            [Phone]
            [MaxLength(20)]
            public string? Phone { get; set; }

            public string? Address { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class UserAuthDto
        {
            [Required]
            public int Id { get; set; }

            [Required]
            [MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [MaxLength(50)]
            public string LastName { get; set; } = string.Empty;
            [Required]
            [EmailAddress]
            [MaxLength(150)]
            public string Email { get; set; } = string.Empty;
            [Required]
            [MaxLength(255)]
            public string Passwordhash { get; set; } = string.Empty;
            [Required]
            [MaxLength(100)]
            public string Role { get; set; } = string.Empty;
        }
        private static UserAuthDto MapToUserAuthDto(SqlDataReader reader)
        {
            return new UserAuthDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Passwordhash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };
        }
        private static UserResponseDto MapToUserDto(SqlDataReader reader)
        {
            return new UserResponseDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                    ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address"))
                    ? null : reader.GetString(reader.GetOrdinal("Address")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
             
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };
        }

        public static async Task<UserResponseDto?> GetUserById(int id)
        {
            Log.Information("DAL: Getting user by ID {Id}", id);
            UserResponseDto? user = null;

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_GetUserById", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    user = MapToUserDto(reader);
                    Log.Information("DAL: User {Id} found", id);
                }
                else
                {
                    Log.Warning("DAL: User {Id} not found", id);
                }

                return user;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error getting user {Id}", id);
                throw;
            }
        }

        // Get User By Email
        public static async Task<UserAuthDto?> GetUserByEmail(string email)
        {
            Log.Information("DAL: Getting user by email {Email}", email);
            UserAuthDto? user = null;

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_GetUserByEmail", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = email;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    user = MapToUserAuthDto(reader);
                }

                return user;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error getting user by email");
                throw;
            }
        }

        // Check Email Exists
        public static async Task<bool> CheckEmailExists(
            string email, int? excludeId = null)
        {
            Log.Information("DAL: Checking email '{Email}' exists", email);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_CheckEmailExists", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = email;
                cmd.Parameters.Add("@ExcludeId", SqlDbType.Int).Value =
                    (object?)excludeId ?? DBNull.Value;

                var existsParam = cmd.Parameters.Add("@Exists", SqlDbType.Bit);
                existsParam.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool exists = (bool)existsParam.Value;
                Log.Information("DAL: Email '{Email}' exists: {Exists}", email, exists);

                return exists;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error checking email");
                throw;
            }
        }

        // Add New User
        public static async Task<int> RegisterNewUser(UseCreateDto dto)
        {
            Log.Information("DAL: Adding new user {Email}", dto.Email);
            string PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);
            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_AddNewUser", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = dto.FirstName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = dto.LastName;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = dto.Email;
                cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 20).Value =
                    (object?)dto.Phone ?? DBNull.Value;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, -1).Value =
                    (object?)dto.Address ?? DBNull.Value;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = PasswordHash;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar, 100).Value = dto.Role;

                var newIdParam = cmd.Parameters.Add("@NewId", SqlDbType.Int);
                newIdParam.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                int newId = (int)newIdParam.Value;

                if (newId > 0)
                    Log.Information("DAL: User added successfully. Id: {Id}", newId);
                else if (newId == -1)
                    Log.Warning("DAL: Email already exists");

                return newId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error adding user");
                throw;
            }
        }

        // Update User
        public static async Task<int> UpdateUser(UserResponseDto dto)
        {
            Log.Information("DAL: Updating user {Id}", dto.Id);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_UpdateUser", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = dto.Id;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = dto.FirstName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = dto.LastName;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = dto.Email;
                cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 20).Value =
                    (object?)dto.Phone ?? DBNull.Value;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, -1).Value =
                    (object?)dto.Address ?? DBNull.Value;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar, 100).Value = dto.Role;

                var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                int result = (int)returnParam.Value;
                Log.Information("DAL: Update result: {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error updating user");
                throw;
            }
        }
        public static async Task<int> ChangeUserPassword(int userId, string newPassword)
        {
            Log.Information("DAL: Updating password for user {Id}", userId);
          
            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_ChangePassword", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@NewPasswordHash", SqlDbType.VarChar, 255).Value = newPassword;
                var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                int result = (int)returnParam.Value;
                Log.Information("DAL: Password update result for user {Id}: {Result}", userId, result);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error updating password for user {Id}", userId);
                throw;
            }
        }
        // Delete User
        public static async Task<int> DeleteUser(int id)
        {
            Log.Information("DAL: Deleting user {Id}", id);

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_DeleteUser", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return (int)returnParam.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error deleting user");
                throw;
            }
        }

        // Get All Users (Paged)
        public static async Task<PagedList<UserResponseDto>> GetAllUsersPaged(
            PaginationParams pagination)
        {
            Log.Information("DAL: Getting users page {Page}", pagination.PageNumber);
            var users = new List<UserResponseDto>();
            int totalCount = 0;

            try
            {
                using SqlConnection conn = new(clsAccessSettings.ConnectionString);
                using SqlCommand cmd = new("sp_GetAllUsersPaged", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pagination.PageNumber;
                cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pagination.PageSize;

                var totalCountParam = new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(totalCountParam);

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(MapToUserDto(reader));
                }

                if (totalCountParam.Value is int tc)
                    totalCount = tc;

                Log.Information("DAL: Retrieved {Count} users", users.Count);

                return new PagedList<UserResponseDto>(
                    users, totalCount, pagination.PageNumber, pagination.PageSize);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DAL: Error getting users");
                throw;
            }
        }







    }
}
