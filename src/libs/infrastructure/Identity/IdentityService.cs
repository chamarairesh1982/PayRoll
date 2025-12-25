using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Payroll.Application.Common.Interfaces;

namespace Payroll.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IConfiguration _configuration;

    public IdentityService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<(string Token, string UserId, string TenantId)> LoginAsync(string email, string password)
    {
        string userId;
        string tenantId;
        string role;

        // SIMULATION: Hardcoded users for Phase 1
        if (email == "admin@tenant1.com" && password == "pass123")
        {
            userId = "user-1";
            tenantId = "00000000-0000-0000-0000-000000000001";
            role = "Admin";
        }
        else if (email == "manager@tenant2.com" && password == "pass123")
        {
            userId = "user-2";
            tenantId = "00000000-0000-0000-0000-000000000002";
            role = "PayrollManager";
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = _configuration["JwtSettings:Secret"];
        
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JWT Secret is missing in configuration.");

        var key = Encoding.ASCII.GetBytes(secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("tenant_id", tenantId) 
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult((tokenHandler.WriteToken(token), userId, tenantId));
    }
}
