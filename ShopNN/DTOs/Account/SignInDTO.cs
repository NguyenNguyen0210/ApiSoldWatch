using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs.Account
{
    public class SignInDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]

        public string Password { get; set; }   
    }
}
