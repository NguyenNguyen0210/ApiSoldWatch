using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class SignInDTO
    {
        [Required]
        public required string Username { get; set; }
        [Required]

        public required string Password { get; set; }
    }
}