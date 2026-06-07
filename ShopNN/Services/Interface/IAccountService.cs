using Microsoft.AspNetCore.Identity;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;

namespace ShopNN.Services.Interface
{
    public interface IAccountService
    {
        Task<UserProfileResponseDTO> SignUp(SignUpDTO dto);
        Task<TokenResponseDTO> SignIn(SignInDTO dto);
        Task SignOut(RefreshTokenRequestDTO request);
        Task<TokenResponseDTO> RefreshToken(RefreshTokenRequestDTO request);
        Task<UserProfileResponseDTO> FindByUserId(string userId);
    }
}

