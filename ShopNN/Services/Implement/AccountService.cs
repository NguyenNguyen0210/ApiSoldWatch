using Microsoft.AspNetCore.Identity;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;

namespace ShopNN.Services.Implement
{
    public class AccountService : IAccountService
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountService(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =========================
        // SIGN IN
        // =========================
        public async Task<(string accessToken, string refreshToken)> SignIn(SignInDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user == null)
                return (string.Empty, string.Empty);

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                return (string.Empty, string.Empty);

            var accessToken = await _authService.GenerateAccessTokenAsync(user);
            var refreshToken = await _authService.GenerateRefreshTokenAsync();

            await _authService.SaveRefreshTokenAsync(refreshToken, user);

            return (accessToken, refreshToken);
        }

        // =========================
        // SIGN UP
        // =========================
        public async Task<IdentityResult> SignUp(SignUpDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return result;

            // 🔥 đảm bảo role tồn tại
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                var roleResult = await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "User"
                });

                if (!roleResult.Succeeded)
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Failed to create role"
                    });
            }

            // 🔥 add role
            var addRoleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!addRoleResult.Succeeded)
                return addRoleResult;

            return result;
        }
    }
}