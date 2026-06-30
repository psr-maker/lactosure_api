using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Lactosure_api.Models.lacto;

namespace Lactosure_api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(int userId, string email,string name)
        {
            var claims = new[]
    {
        new Claim("id", userId.ToString()),
        new Claim("email", email),
        new Claim("name", name)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expiryHours = Convert.ToDouble(
                _configuration["Jwt:ExpiryHours"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(expiryHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}