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

        public AccountService(
            IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        // =========================
        // SIGN IN
        // =========================
        public async Task<(string accessToken, string RefreshToken)> SignIn(SignInDTO dto)

        {
            var user = await _userManager.FindByNameAsync(dto.Username);



            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (user != null && valid)
            {

                // 🔥 tạo token
                var accessToken = await _authService.GenerateAccessTokenAsync(user);
                var refreshToken = await _authService.GenerateRefreshTokenAsync();

                // 🔥 lưu refresh token
                await _authService.SaveRefreshTokenAsync(refreshToken, user);

                return (accessToken, refreshToken);
            }
            return (null, null);
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

            // ❗ chỉ add role khi tạo user thành công
            if (!result.Succeeded)
            {
                return result;
            }

            await _userManager.AddToRoleAsync(user, "User");

            return result;
        }


    }
}