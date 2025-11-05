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

        // ✅ 1️⃣ Login endpoint: Generate JWT
       
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            if (userLogin == null)
                return BadRequest(new { message = "Request body is required." });

            if (userLogin.Username == "admin" && userLogin.Password == "password")
            {
                var token = GenerateJwtToken(userLogin.Username);
                return Ok(new { token });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        // ✅ 2️⃣ Protected endpoint: Requires valid token
        [Authorize]
        [HttpGet("getemployees")]
        public IActionResult GetEmployees()
        {
            var username = User.Identity?.Name; // Extract username from token claim
            return Ok(new
            {
                message = $"Token valid. Welcome {username}!",
                employees = new[] { "John", "Emma", "David" }
            });
        }

        // ✅ 3️⃣ Token generation logic
        private string GenerateJwtToken(string username)
        {
            var key = _config["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

