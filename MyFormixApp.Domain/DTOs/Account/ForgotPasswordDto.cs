using System.ComponentModel.DataAnnotations;

namespace MyFormixApp.Domain.DTOs.Account
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}