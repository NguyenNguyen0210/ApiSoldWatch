using Microsoft.AspNetCore.Identity;
using ShopNN.DTOs;
using ShopNN.Entities;

namespace ShopNN.Services.Interface
{
    public interface IAccountService
    {
        Task<IdentityResult> SignUp(SignUpDTO dto);
        Task<TokenResponseDTO> SignIn(SignInDTO dto);
        Task SignOut(RefreshTokenRequestDTO request);
        Task<TokenResponseDTO> RefreshToken(RefreshTokenRequestDTO request);
        Task<ApplicationUser> FindByUserId(string userId);



    }
}
