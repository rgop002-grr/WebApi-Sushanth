using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi_Sushanth.Models;

namespace JwtTokenDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }

        // Login endpoint: generate JWT token (no protected endpoints here)
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            if (userLogin == null)
                return BadRequest(new { message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // NOTE: Replace this hard-coded check with your real user validation (DB + hashed password)
            if (userLogin.Username == "admin" && userLogin.Password == "password")
            {
                var token = GenerateJwtToken(userLogin.Username);
                return Ok(new { token });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Token generation method (keeps it inside controller for demo; you can move to a service)
        private string GenerateJwtToken(string username)
        {
            // Prefer storing this key in appsettings or environment variables (do NOT hard-code in production)
            var keyFromConfig = _config["Jwt:Key"];
            var signingKey = string.IsNullOrWhiteSpace(keyFromConfig)
                ? "ThisIsMySuperSecretKeyForJwtToken12345" // fallback for quick local testing
                : keyFromConfig;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "myApi",
                audience: _config["Jwt:Audience"] ?? "myApiUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
