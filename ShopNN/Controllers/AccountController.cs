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
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }

        }
    }
}