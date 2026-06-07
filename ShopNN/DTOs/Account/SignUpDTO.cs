using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs.Account
{
    public class SignUpDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]

        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
