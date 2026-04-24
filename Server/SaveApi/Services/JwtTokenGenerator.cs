using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SaveApi.Services;

public static class JwtTokenGenerator
{
    public static string Generate(
        string login,
        string nome,
        string key,
        string issuer,
        string audience
    )
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.GivenName, nome),
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
