using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Current.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Current.Api.Tests.Helpers;

public static class TestAuthHelper
{
    public static string CreateAccessToken(User user)
    {
        var tokenSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSettings.Key));
        var tokenCredentials = new SigningCredentials(tokenSecurityKey, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var jwtToken = new JwtSecurityToken(
            issuer: TestJwtSettings.Issuer,
            audience: TestJwtSettings.Audience,
            claims: tokenClaims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: tokenCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}
