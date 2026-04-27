using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;

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

        // =========================
        // SIGN UP
        // =========================
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

        // =========================
        // SIGN IN
        // =========================
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (accessToken, refreshToken) = await _accountService.SignIn(dto);

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password"
                });
            }

            return Ok(new
            {
                accessToken,
                refreshToken
            });
        }
    }
}