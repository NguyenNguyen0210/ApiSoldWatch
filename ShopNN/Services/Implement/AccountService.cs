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

        public async Task<TokenResponseDTO> SignIn(SignInDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user == null)
                throw new Exception("User not found");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                throw new Exception("Password Incorrect");
            var token = await _authService.GenerateTokenAsync(user);

            return token;
        }

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

            var addRoleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!addRoleResult.Succeeded)
                return addRoleResult;

            return result;
        }


        public async Task<TokenResponseDTO> RefreshToken(RefreshTokenRequestDTO request)
        {
            var tokenRespone = await _authService.RefreshToken(request.Token);
            return tokenRespone;

        }

        public async Task SignOut(RefreshTokenRequestDTO refreshToken)
        {
            await _authService.Revoke(refreshToken.Token);

        }

        public async Task<ApplicationUser> FindByUserId(string userId)
        {
            var profile = await _userManager.FindByIdAsync(userId);
            if(profile == null) return null;
            return profile;
        }
    }
}