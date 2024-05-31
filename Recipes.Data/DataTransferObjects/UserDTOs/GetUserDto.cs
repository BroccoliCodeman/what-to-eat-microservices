namespace Recipes.Data.DataTransferObjects.UserDTOs
{
    public class GetUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Avatar { get; set; } = null!;
        public List<SavedRecipeDto> SavedRecipes { get; } = new List<SavedRecipeDto>();
    }
}