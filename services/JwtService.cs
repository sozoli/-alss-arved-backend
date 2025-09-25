using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace alss_invoice_back.services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string username, int userId)
{
    var jwtSettings = _configuration.GetSection("Jwt");

    var key = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");
    var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing in configuration.");
    var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is missing in configuration.");

    var claims = new[]
    {
        new Claim("Id", userId.ToString()), // Добавляем ID пользователя
        new Claim(ClaimTypes.Name, username), 
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: signingCredentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
    }
}