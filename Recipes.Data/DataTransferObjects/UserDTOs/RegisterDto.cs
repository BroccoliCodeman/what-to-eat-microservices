namespace Recipes.Data.DataTransferObjects.UserDTOs
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar {  get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordRepeat {  get; set; } = string.Empty;
    }
}