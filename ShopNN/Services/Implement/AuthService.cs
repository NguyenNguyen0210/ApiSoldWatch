using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
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
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration config)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _config = config;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? "Unknown"),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
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

            await _refreshTokenRepository.AddAsync(entity);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task Revoke(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken)
                ?? throw new SecurityTokenException("Refresh token not found");

            token.IsRevoked = true;
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task<TokenResponseDTO> RefreshToken(string refreshToken)
        {
            // Dùng GetActiveByTokenAsync — đã filter revoked + expired + include User
            var existing = await _refreshTokenRepository.GetActiveByTokenAsync(refreshToken)
                ?? throw new SecurityTokenException("Invalid or expired refresh token.");

            var user = existing.User
                ?? await _userManager.FindByIdAsync(existing.UserId.ToString())
                ?? throw new Exception("User not found");

            existing.IsRevoked = true;
            await _refreshTokenRepository.SaveChangesAsync();

            return await GenerateTokenAsync(user);
        }

        public async Task<TokenResponseDTO> GenerateTokenAsync(ApplicationUser user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString();
            await SaveRefreshTokenAsync(refreshToken, user);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}