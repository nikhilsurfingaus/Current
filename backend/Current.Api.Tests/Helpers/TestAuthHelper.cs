using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Current.Api.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Current.Api.Tests.Helpers;

public static class TestAuthHelper
{
    public static string CreateAccessToken(User user, IConfiguration configuration)
    {
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var tokenSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var tokenCredentials = new SigningCredentials(tokenSecurityKey, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var jwtToken = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: tokenClaims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: tokenCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}
