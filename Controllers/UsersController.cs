using EcommerceBuisnessLayer;
using EcommerceDataLayer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using static EcommerceDataLayer.clsUserData;

namespace EcommrceApi.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ===============================
        // 🔹 Get User By Id
        // ===============================
        [HttpGet("{id}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserById(int id)
        {
            Log.Information("API: Request to get user {Id}", id);

            if (id <= 0)
                return BadRequest("Invalid User Id");

            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"User with id {id} not found");

            return Ok(user);
        }

        // ===============================
        // 🔹 Get All Users (Paged)
        // ===============================
        [HttpGet("GetAll", Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParams pagination)
        {
            Log.Information("API: Request to get all users");

            var pagedList = await _userService.GetAllUsers(pagination);

            var metaData = new
            {
                pagedList.MetaData.TotalCount,
                pagedList.MetaData.PageSize,
                pagedList.MetaData.CurrentPage,
                pagedList.MetaData.TotalPages,
                pagedList.MetaData.HasNext,
                pagedList.MetaData.HasPrevious
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(pagedList);
        }

       
    
        // ===============================
        [HttpPost("AddNewUser", Name = "AddNewUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNewUser([FromBody] UseCreateDto dto)
        {
            Log.Information("API: Adding new user {Email}", dto?.Email);

            if (dto == null)
                return BadRequest(new { message = "Invalid user data." });

            var result = await _userService.RegisterNewUser(dto);

            return result switch
            {
                UserOperationResult.Success =>
                    CreatedAtRoute("GetUserById", new { id = dto.Id }, dto),

                UserOperationResult.DuplicateEmail =>
                    BadRequest(new { message = "Email already exists." }),

                UserOperationResult.InvalidData =>
                    BadRequest(new { message = "Invalid user data." }),

                _ =>
                    StatusCode(500, new { message = "Failed to create user." })
            };
        }

        // ===============================
        // 🔹 Update User
        // ===============================
        [HttpPut("Update/{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserResponseDto dto)
        {
            Log.Information("API: Updating user {Id}", id);

            if (dto == null || id != dto.Id)
                return BadRequest("Invalid Id or mismatched data");

            var result = await _userService.UpdateUser(dto);

            return result switch
            {
                UserOperationResult.Success => Ok(dto),
                UserOperationResult.NotFound => NotFound("User not found"),
                UserOperationResult.DuplicateEmail => BadRequest("Email already exists"),
                UserOperationResult.InvalidData => BadRequest("Invalid data"),
                _ => StatusCode(500, "Unexpected error")
            };
        }

        // ===============================
        // 🔹 Delete User
        // ===============================
        [HttpDelete("{id}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            Log.Information("API: Deleting user {Id}", id);

            var result = await _userService.DeleteUser(id);

            return result switch
            {
                UserOperationResult.Success => Ok(new { message = "User deleted successfully" }),
                UserOperationResult.NotFound => NotFound("User not found"),
                UserOperationResult.InvalidData => BadRequest("Invalid Id"),
                _ => StatusCode(500, "Delete failed")
            };
        }
    }





}

