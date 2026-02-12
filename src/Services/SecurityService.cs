using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KidFit.Services
{
    public class JwtParams
    {
        public string Identifier { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string GivenName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Issuer { get; set; } = "KidFit";
        public string Audience { get; set; } = "KidFitClient";
        public int ExpiresInMinutes { get; set; } = 60;
    }

    public class SecurityService
    {
        public static string GenerateJWTToken(JwtParams param, string secret)
        {
            // Create token claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, param.Identifier),
                new(ClaimTypes.Name, param.Name),
                new(ClaimTypes.Email, param.Email),
                new(ClaimTypes.GivenName, param.GivenName),
                new(ClaimTypes.Role, param.Role),
            };

            // Create signing key 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            // Choose signing algorithm 
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Choose expired time for token 
            var expired = DateTime.UtcNow.AddMinutes(param.ExpiresInMinutes);

            // Create token 
            var token = new JwtSecurityToken(
                    issuer: param.Issuer,
                    audience: param.Audience,
                    claims: claims,
                    expires: expired,
                    signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
