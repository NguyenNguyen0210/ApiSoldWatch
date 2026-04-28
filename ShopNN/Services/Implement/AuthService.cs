using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopNN.DTOs;
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

        public async Task SaveRefreshTokenAsync(string refreshToken, ApplicationUser user)
        {
            var entity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Revoke(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null)
                throw new Exception("Refresh token not found");

            token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }

        public async Task<TokenResponseDTO> RefreshToken(string refreshtoken)
        {
            var existing = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshtoken);


            if (existing == null || existing.IsRevoked || existing.ExpiryDate < DateTime.UtcNow) throw new Exception("Invalid or expired refresh token.");
            ApplicationUser? user = await _userManager.FindByIdAsync(existing.UserId.ToString());
            if (user == null) throw new Exception("User not found");


            existing.IsRevoked = true;

            TokenResponseDTO tokennew = await GenerateTokenAsync(user);



            return tokennew;
        }

        public async Task<TokenResponseDTO> GenerateTokenAsync(ApplicationUser user)
        {
            string accessToken = await GenerateAccessTokenAsync(user);
            string refreshToken = Guid.NewGuid().ToString();
            await SaveRefreshTokenAsync(refreshToken, user);
            return new TokenResponseDTO { refreshToken = refreshToken, accessToken = accessToken };
        }
    }
}