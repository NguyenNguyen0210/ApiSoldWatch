using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Enums;
using System;

namespace ShopNN.Services.Implement
{
    public class AccountService : IAccountService
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IEmailService emailService,
            ILogger<AccountService> logger)
        {
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<TokenResponseDTO> SignIn(SignInDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user == null)
                throw new NotFoundException("User not found");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                throw new BadRequestException("Password Incorrect");

            var token = await _authService.GenerateTokenAsync(user);

            return token;
        }

        public async Task<UserProfileResponseDTO> SignUp(SignUpDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Đăng ký thất bại: {errors}");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);

            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"Gán quyền thất bại: {errors}");
            }

            try
            {
                var emailSubject = "Chào mừng bạn đến với ShopNN!";
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                        <h2 style='color: #b3923b; text-align: center;'>Chào mừng thành viên mới!</h2>
                        <p>Xin chào <strong>{user.UserName}</strong>,</p>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>ShopNN — Cửa hàng đồng hồ cao cấp</strong>.</p>
                        <p>Tài khoản của bạn đã được khởi tạo thành công với email <strong>{user.Email}</strong>. Bây giờ bạn đã có thể đăng nhập vào hệ thống để mua sắm các mẫu đồng hồ sang trọng và thời thượng nhất.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='http://localhost:5173/login' style='background-color: #b3923b; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Đăng Nhập Ngay</a>
                        </div>
                        <hr style='border: 0; border-top: 1px solid #eeeeee;' />
                        <p style='font-size: 12px; color: #777777; text-align: center;'>Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email hoặc liên hệ với bộ phận hỗ trợ của chúng tôi.<br/>© {DateTime.UtcNow.Year} ShopNN. All rights reserved.</p>
                    </div>";

                await _emailService.SendEmailAsync(user.Email!, emailSubject, emailBody, user.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể gửi email chúc mừng đăng ký thành công cho {Email}.", user.Email);
            }

            return new UserProfileResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = new List<string> { RoleNames.User }
            };
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

        public async Task<UserProfileResponseDTO> FindByUserId(string userId)
        {
            var profile = await _userManager.FindByIdAsync(userId);
            if (profile == null) throw new NotFoundException("User not found");
            
            var roles = await _userManager.GetRolesAsync(profile);

            return new UserProfileResponseDTO
            {
                Id = profile.Id,
                UserName = profile.UserName ?? string.Empty,
                Email = profile.Email ?? string.Empty,
                Roles = roles
            };
        }
    }
}
