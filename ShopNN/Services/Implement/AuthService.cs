using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
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
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _config = config;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            _logger.LogInformation("Generating Access Token for User ID: {UserId}", user.Id);
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

            var writtenToken = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("Successfully generated Access Token for User ID: {UserId}", user.Id);
            return writtenToken;
        }

        public async Task SaveRefreshTokenAsync(string refreshToken, ApplicationUser user)
        {
            _logger.LogInformation("Saving new Refresh Token for User ID: {UserId}", user.Id);
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
            _logger.LogInformation("Successfully saved Refresh Token for User ID: {UserId}", user.Id);
        }

        public async Task Revoke(string refreshToken)
        {
            _logger.LogInformation("Attempting to revoke Refresh Token");
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null)
            {
                _logger.LogWarning("Revocation failed: Refresh token not found in the database.");
                throw new SecurityTokenException("Refresh token not found");
            }

            token.IsRevoked = true;
            await _refreshTokenRepository.SaveChangesAsync();
            _logger.LogInformation("Successfully revoked Refresh Token for User ID: {UserId}", token.UserId);
        }

        public async Task<TokenResponseDTO> RefreshToken(string refreshToken)
        {
            _logger.LogInformation("Attempting to refresh token using Refresh Token");
            var existing = await _refreshTokenRepository.GetActiveByTokenAsync(refreshToken);
            if (existing == null)
            {
                _logger.LogWarning("Token refresh failed: Invalid or expired Refresh Token");
                throw new SecurityTokenException("Invalid or expired refresh token.");
            }

            var user = existing.User
                ?? await _userManager.FindByIdAsync(existing.UserId.ToString());

            if (user == null)
            {
                _logger.LogWarning("Token refresh failed: User with ID {UserId} not found", existing.UserId);
                throw new Exception("User not found");
            }

            var newToken = await GenerateTokenAsync(user);
            existing.IsRevoked = true;
            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully refreshed token for User ID: {UserId}", user.Id);
            return newToken;
        }

        public async Task<TokenResponseDTO> GenerateTokenAsync(ApplicationUser user)
        {
            _logger.LogInformation("Generating token response (access and refresh token) for User ID: {UserId}", user.Id);
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
