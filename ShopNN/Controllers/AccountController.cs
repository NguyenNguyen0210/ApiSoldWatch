using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using System.Security.Claims;

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
                return BadRequest(ApiResponse.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _accountService.SignUp(dto);

            if (!result.Succeeded)
                return BadRequest(ApiResponse.FailureResult("Sign up failed", result.Errors.Select(e => e.Description).ToList()));

            return Ok(ApiResponse.SuccessResult("Sign up success"));
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            try
            {
                var tokenResponse = await _accountService.SignIn(dto);
                return Ok(ApiResponse<TokenResponseDTO>.SuccessResult(tokenResponse, "Sign in success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

                var response = await _accountService.RefreshToken(dto);
                return Ok(ApiResponse<TokenResponseDTO>.SuccessResult(response, "Token refreshed"));
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(ApiResponse.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut([FromBody] RefreshTokenRequestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ApiResponse.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                await _accountService.SignOut(dto);
                return Ok(ApiResponse.SuccessResult("Signed out successfully"));
            }
            catch (SecurityTokenException ex) { return Unauthorized(ApiResponse.FailureResult(ex.Message)); }
            catch (Exception ex) { return BadRequest(ApiResponse.FailureResult(ex.Message)); }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.FailureResult("Invalid token."));
                }

                var userProfile = await _accountService.FindByUserId(userId);
                if (userProfile == null)
                {
                    return NotFound(ApiResponse.FailureResult("User not found."));
                }

                return Ok(ApiResponse<object>.SuccessResult(userProfile, "Profile retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.FailureResult("An error occurred while retrieving the profile.", new List<string> { ex.Message }));
            }
        }
    }
}