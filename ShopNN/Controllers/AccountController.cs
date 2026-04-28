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
                return BadRequest(ModelState);

            var result = await _accountService.SignUp(dto);

            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = "Sign up failed",
                    errors = result.Errors.Select(e => e.Description)
                });

            return Ok(new
            {
                message = "Sign up success"
            });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var tokenResponse = await _accountService.SignIn(dto);
                return Ok(new
                {
                    tokenResponse.accessToken,
                    tokenResponse.refreshToken
                });
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message
                    
                    );
            }

        }


        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _accountService.RefreshToken(dto);
                return Ok(response);
            }
            catch(SecurityTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut([FromBody] RefreshTokenRequestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                await _accountService.SignOut(dto);
                return Ok(new { message = "Signed out successfully" });
            }
            catch (SecurityTokenException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }

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
                    return Unauthorized(new { message = "Invalid token." });
                }

                var userProfile = await _accountService.FindByUserId(userId);
                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "An error occurred while retrieving the profile.", error = ex.Message });
            }
        }
    }
}