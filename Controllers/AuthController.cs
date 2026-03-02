using EcommerceBuisnessLayer;
using EcommerceBuisnessLayer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommrceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("API: Login attempt for email: {Email}", loginDto.Email);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByEmail(loginDto.Email);

            // التحقق من المستخدم والباسورد
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Passwordhash))
            {
                _logger.LogWarning("API: Failed login attempt for email: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // 1. تجهيز الـ Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
               new Claim(ClaimTypes.Role, user.Role) 
            };

          
            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
            {
                 _logger.LogError("API: JWT Key is missing in configuration.");
                return StatusCode(500, "Internal server error.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. إنشاء الـ Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"], // يفضل قراءتها من الكونفيج
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2), // ساعتين مدة مناسبة
                signingCredentials: creds
            );

            Log.Information("API: Successful login for User ID: {Id}", user.Id);
            
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userId = user.Id,
                name = $"{user.FirstName} {user.LastName}"
            });
        }
    }
}