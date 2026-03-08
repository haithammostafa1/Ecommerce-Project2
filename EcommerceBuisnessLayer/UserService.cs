using EcommerceDataLayer;
using EcommerceDataLayer.Helpers;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceDataLayer.clsUserData;

namespace EcommerceBuisnessLayer
{
    public interface IUserRepository
    {
        Task<UserResponseDto?> GetUserById(int id);
        Task<int> UpdateUserRefreshToken(int userId, string? refreshTokenHash, DateTime? expiresAt, DateTime? revokedAt);
        Task<UserAuthDto?> GetUserByEmail(string email);
        Task<PagedList<UserResponseDto>> GetAllUsers(PaginationParams pagination);
        Task<int> RegisterNewUser(UseCreateDto dto);
        Task<int> UpdateUser(UserResponseDto dto);
        Task<int> DeleteUser(int id);
        Task<int> ChangeUserPassword(int id,string NewPassword);
        Task<bool> CheckEmailExists(string email, int? excludeId = null);
    }
    public class UserRepository : IUserRepository
    {
        public async Task<UserResponseDto?> GetUserById(int id)
            => await clsUserData.GetUserById(id);
        public async Task<int> UpdateUserRefreshToken(int userId, string? refreshTokenHash, DateTime? expiresAt, DateTime? revokedAt)
            =>await clsUserData.UpdateUserRefreshToken(userId, refreshTokenHash, expiresAt, revokedAt);
        public async Task<int> ChangeUserPassword(int id,string NewPassword)
            => await clsUserData.ChangeUserPassword(id, NewPassword);
      
        public async Task<UserAuthDto?> GetUserByEmail(string email)
            => await clsUserData.GetUserByEmail(email);

        public async Task<PagedList<UserResponseDto>> GetAllUsers(PaginationParams pagination)
            => await clsUserData.GetAllUsersPaged(pagination);

        public async Task<int> RegisterNewUser(UseCreateDto dto)
            => await clsUserData.RegisterNewUser(dto);

        public async Task<int> UpdateUser(UserResponseDto dto)
            => await clsUserData.UpdateUser(dto);

        public async Task<int> DeleteUser(int id)
            => await clsUserData.DeleteUser(id);

        public async Task<bool> CheckEmailExists(string email, int? excludeId = null)
            => await clsUserData.CheckEmailExists(email, excludeId);
    }
    public enum UserOperationResult
    {
        Success = 1,
        NotFound = 0,
        DuplicateEmail = -1,
        InvalidData = -2,
        Failed = -99
    }
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserById(int id);
        Task<UserAuthDto?> GetUserByEmail(string email);
        Task<UserOperationResult> UpdateUserRefreshToken(int userId, string? refreshTokenHash, DateTime? expiresAt, DateTime? revokedAt);
        Task<PagedList<UserResponseDto>> GetAllUsers(PaginationParams pagination);
        Task<UserOperationResult> RegisterNewUser(UseCreateDto dto);
        Task<UserOperationResult> ChangeUserPassword(int id, string password);
        Task<UserOperationResult> UpdateUser(UserResponseDto dto);
        Task<UserOperationResult> DeleteUser(int id);
        Task<bool> CheckEmailExists(string email);
    }
    public class UserService: IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository,
                           ILogger<UserService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<UserResponseDto?> GetUserById(int id)
        {
            _logger.LogInformation("Service: Getting user {Id}", id);
            return await _repository.GetUserById(id);
        }
        public async Task<UserOperationResult> UpdateUserRefreshToken(int userId, string? refreshTokenHash, DateTime? expiresAt, DateTime? revokedAt)
        {
            if(userId<=0)
            {
                return UserOperationResult.InvalidData;
            }

            int result= await _repository.UpdateUserRefreshToken(userId, refreshTokenHash, expiresAt, revokedAt);

            return result > 0
        ? UserOperationResult.Success
        : UserOperationResult.Failed;

        }
        public async Task<UserAuthDto?> GetUserByEmail(string email)
        {
            _logger.LogInformation("Service: Getting user by email {Email}", email);
            return await _repository.GetUserByEmail(email);
        }

        public async Task<PagedList<UserResponseDto>> GetAllUsers(PaginationParams pagination)
        {
            _logger.LogInformation("Service: Getting all users");
            return await _repository.GetAllUsers(pagination);
        }

        public async Task<UserOperationResult> RegisterNewUser(UseCreateDto dto)
        {
            _logger.LogInformation("Service: Adding user {Email}", dto.Email);

           

            if (await _repository.CheckEmailExists(dto.Email))
                return UserOperationResult.DuplicateEmail;

            int result = await _repository.RegisterNewUser(dto);

            return result > 0
                ? UserOperationResult.Success
                : UserOperationResult.Failed;
        }

        public async Task<UserOperationResult> UpdateUser(UserResponseDto dto)
        {
            if (dto.Id <= 0)
                return UserOperationResult.InvalidData;

            var existing = await _repository.GetUserById(dto.Id);
            if (existing == null)
                return UserOperationResult.NotFound;

            if (await _repository.CheckEmailExists(dto.Email, dto.Id))
                return UserOperationResult.DuplicateEmail;

            int result = await _repository.UpdateUser(dto);

            return result == 1
                ? UserOperationResult.Success
                : UserOperationResult.Failed;
        }
       public async Task<UserOperationResult> ChangeUserPassword(int id, string password)
        {
            if(id<= 0) return UserOperationResult.InvalidData;
            if(string.IsNullOrWhiteSpace(password) || password.Length<6)
                return UserOperationResult.InvalidData;
            var  existing= await _repository.GetUserById(id);
            if(existing == null) return UserOperationResult.NotFound;
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            int result = await _repository.ChangeUserPassword(id, hashedPassword);

            return result ==1?UserOperationResult.Success: UserOperationResult.Failed;

        }
        public async Task<UserOperationResult> DeleteUser(int id)
        {
            if (id <= 0)
                return UserOperationResult.InvalidData;

            int result = await _repository.DeleteUser(id);

            return result == 1
                ? UserOperationResult.Success
                : UserOperationResult.NotFound;
        }

        public async Task<bool> CheckEmailExists(string email)
            => await _repository.CheckEmailExists(email);
    }


}




    

