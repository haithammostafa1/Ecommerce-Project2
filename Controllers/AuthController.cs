using EcommerceBuisnessLayer;
using EcommrceApi.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EcommrceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class AuthController : ControllerBase
    {
        private static string GenerateRefreshToken()//Hash Generator
        {
            var bytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
        private static string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var secretKey = _configuration["Jwt:Key"];
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                ValidateLifetime = false // 👈 السر هنا! بنقول للسيستم اقبل التوكن حتى لو وقته خلص
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            // نتأكد إن التوكن ده متولد بنفس خوارزمية التشفير بتاعتنا
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token format");
            }

            return principal;
        }
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }
        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { message = "Invalid client request" });
            var principle = GetPrincipalFromExpiredToken(request.Token);
            if (principle == null)
                return BadRequest(new { message = "Invalid access token or refresh token" });
            var email = principle.FindFirstValue(ClaimTypes.Email);
            var user = await _userService.GetUserByEmail(email!);

            if (user == null)
                return BadRequest(new { message = "User not found" });

            var incomingTokenHash = HashRefreshToken(request.Token);

            if (user.RefreshTokenHash != incomingTokenHash ||
                user.RefreshTokenExpiresAt <= DateTime.UtcNow ||
                user.RefreshTokenRevokedAt != null)
            {
                _logger.LogWarning("API: Invalid or expired refresh token for user {Email}", email);
                return Unauthorized(new { message = "Invalid or expired refresh token. Please login again." });
            }

            // 4. لو كل حاجة تمام، نولد Access Token جديد
            var secretKey = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: principle.Claims, // بناخد نفس الكلايمز القديمة
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(newToken);

            // 5. (إختياري ومستحسن) نولد Refresh Token جديد برضه زيادة أمان (Token Rotation)
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashRefreshToken(newRefreshToken);
            await _userService.UpdateUserRefreshToken(user.Id, newRefreshTokenHash, DateTime.UtcNow.AddDays(7), null);

            _logger.LogInformation("API: Token refreshed successfully for user {Email}", email);

            // 6. نرجع الداتا الجديدة للعميل
            return Ok(new
            {
                token = newAccessToken,
                expiration = newToken.ValidTo,
                refreshToken = newRefreshToken
            });


        }
        [HttpPost("Logout")]
        [Authorize] 
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("API: Failed logout attempt - Invalid token claims");
                return Unauthorized(new { message = "Invalid token." });
            }

            _logger.LogInformation("API: Logout attempt for user {Id}", userId);

            await _userService.UpdateUserRefreshToken(userId, null, null, DateTime.UtcNow);

            _logger.LogInformation("API: User {Id} logged out successfully", userId);

            return Ok(new { message = "Logged out successfully." });
        }
        [AllowAnonymous]
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

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"], 
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );
              var accesstoken=new JwtSecurityTokenHandler().WriteToken(token);

              var refreshtoken = GenerateRefreshToken();
              var refreshtokenhash = HashRefreshToken(refreshtoken);

            var refreshtokenexpire=DateTime.UtcNow.AddDays(7);
            //update
            await _userService.UpdateUserRefreshToken(user.Id, refreshtokenhash, refreshtokenexpire, null);
            Log.Information("API: Successful login for User ID: {Id}", user.Id);
            
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                Refreshtoken=refreshtoken,
                userId = user.Id,
                name = $"{user.FirstName} {user.LastName}"
            });
        }
    }
}