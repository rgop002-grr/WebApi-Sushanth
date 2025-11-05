using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1️⃣ Add controller services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ 2️⃣ Add Authentication (JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Safely handle missing keys from appsettings
    var keyFromConfig = builder.Configuration["Jwt:Key"];
    var key = string.IsNullOrWhiteSpace(keyFromConfig)
        ? "ThisIsMySuperSecretKeyForJwtToken12345"  // fallback key
        : keyFromConfig;

    var issuer = builder.Configuration["Jwt:Issuer"] ?? "myApi";
    var audience = builder.Configuration["Jwt:Audience"] ?? "myApiUsers";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

var app = builder.Build();

// ✅ 3️⃣ Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ 4️⃣ Middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ 5️⃣ Run
app.Run();


//I am sushanth
