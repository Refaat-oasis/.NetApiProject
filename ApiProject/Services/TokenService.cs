using ApiProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiProject.Services
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user , bool rememberMe);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<ApplicationUser> _userManager;

        // Dependency Injection to get configuration (like the secret key)
        public TokenService(IConfiguration config , UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
            var secretKey = _config["JWT:Key"] ?? throw new ArgumentNullException("JWT Key is missing");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        }

        public async Task<string> CreateToken(ApplicationUser user , bool rememberMe)
        {
            // Claims are information about the user that we put in the token
            var claims = new List<Claim>
            {
               // new Claim(JwtRegisteredClaimNames.NameId, user.Id),
               //aya
                  new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            };
            //basant
            var expiry = rememberMe
? DateTime.UtcNow.AddDays(7)
: DateTime.UtcNow.AddHours(2);
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            // Credentials for signing the token
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // Token description
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(double.Parse(_config["JWT:DurationInMinutes"] ?? "60")),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
