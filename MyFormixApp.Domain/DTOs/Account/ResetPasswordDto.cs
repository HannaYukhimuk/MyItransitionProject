using System.ComponentModel.DataAnnotations;

namespace MyFormixApp.Domain.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required]
        public string? Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match.")]
        public string? ConfirmPassword { get; set; }
    }
}