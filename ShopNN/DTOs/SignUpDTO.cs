using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class SignUpDTO
    {
        [Required]
        public required string Username { get; set; }
        [Required]

        public required string Password { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}