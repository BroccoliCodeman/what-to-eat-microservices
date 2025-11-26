using System.ComponentModel.DataAnnotations;

namespace Recipes.Data.DataTransferObjects.UserDTOs
{

    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid URL for Avatar.")]
        public string Avatar { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password repeat is required.")]
        [Compare("Password", ErrorMessage = "Password and PasswordRepeat do not match.")]
        public string PasswordRepeat { get; set; } = string.Empty;
    }
}