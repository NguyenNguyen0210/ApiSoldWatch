using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IdentityResult result = await  _accountService.SignUp(dto);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO dto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var (accessToken,RefreshToken) = await _accountService.SignIn(dto);
            if (accessToken == null && RefreshToken == null) { return BadRequest("Không tồn tại User hoặc Sai PW"); }
            return Ok(new {accessToken,RefreshToken});
        }

    }
}
