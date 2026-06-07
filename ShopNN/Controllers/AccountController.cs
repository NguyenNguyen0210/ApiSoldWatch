using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Services.Interface;
using System.Security.Claims;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var profile = await _accountService.SignUp(dto);
            return Ok(ApiResponse<UserProfileResponseDTO>.SuccessResult(profile, "Sign up success"));
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var tokenResponse = await _accountService.SignIn(dto);
            return Ok(ApiResponse<TokenResponseDTO>.SuccessResult(tokenResponse, "Sign in success"));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var response = await _accountService.RefreshToken(dto);
            return Ok(ApiResponse<TokenResponseDTO>.SuccessResult(response, "Token refreshed"));
        }

        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut([FromBody] RefreshTokenRequestDTO dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            
            await _accountService.SignOut(dto);
            return Ok(ApiResponse<object>.SuccessResult("Signed out successfully"));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("Invalid token.");
            }

            var userProfile = await _accountService.FindByUserId(userId);
            return Ok(ApiResponse<object>.SuccessResult(userProfile, "Profile retrieved"));
        }
    }
}
