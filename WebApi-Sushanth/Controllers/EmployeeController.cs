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

        // 🔹 Step 1: Login endpoint to generate JWT token
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            // Normally you'd validate from database
            if (userLogin.Username == "admin" && userLogin.Password == "password")
            {
                var token = GenerateJwtToken(userLogin.Username);
                return Ok(new { Token = token });
            }
            return Unauthorized("Invalid credentials");
        }

        // 🔹 Step 2: A protected endpoint (requires JWT token)
        [Authorize]
        [HttpGet("getemployees")]
        public IActionResult GetEmployees()
        {
            var employees = new[]
            {
                new { Id = 1, Name = "Sushanth", Department = "IT" },
                new { Id = 2, Name = "Rahul", Department = "HR" },
                new { Id = 3, Name = "Anjali", Department = "Finance" }
            };
            return Ok(employees);
        }

        // 🔹 Step 3: JWT Token generation method
        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsMySuperSecretKeyForJwtToken12345"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "myApi",
                audience: "myApiUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    
}
