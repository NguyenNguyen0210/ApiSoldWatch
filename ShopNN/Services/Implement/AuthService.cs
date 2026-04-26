using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopNN.Entities;
using ShopNN.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShopNN.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _context = context;
            _config = config;
        }

        // =========================
        // ACCESS TOKEN (JWT)
        // =========================
        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // =========================
        // REFRESH TOKEN (random)
        // =========================
        public Task<string> GenerateRefreshTokenAsync()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var token = Convert.ToBase64String(randomBytes);

            return Task.FromResult(token);
        }

        // =========================
        // SAVE REFRESH TOKEN
        // =========================
        public async Task SaveRefreshTokenAsync(string refreshToken, ApplicationUser user)
        {
            var entity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // =========================
        // REVOKE TOKEN
        // =========================
        public async Task Revoke(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null)
                throw new Exception("Refresh token not found");

            token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }
    }
}