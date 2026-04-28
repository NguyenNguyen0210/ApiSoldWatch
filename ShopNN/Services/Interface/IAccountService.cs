using Microsoft.AspNetCore.Identity;
using ShopNN.DTOs;
using ShopNN.Entities;

namespace ShopNN.Services.Interface
{
    public interface IAccountService
    {
        Task<IdentityResult> SignUp(SignUpDTO dto);
        Task<TokenResponseDTO> SignIn(SignInDTO dto);

        Task<TokenResponseDTO> RefreshToken(RefreshTokenRequestDTO request);


    }
}
