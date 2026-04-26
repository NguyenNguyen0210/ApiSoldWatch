using Microsoft.AspNetCore.Identity;
using ShopNN.DTOs;
using ShopNN.Entities;

namespace ShopNN.Services.Interface
{
    public interface IAccountService
    {
        Task<IdentityResult> SignUp(SignUpDTO dto);
        Task<(string accessToken, string RefreshToken)> SignIn(SignInDTO dto);


    }
}
