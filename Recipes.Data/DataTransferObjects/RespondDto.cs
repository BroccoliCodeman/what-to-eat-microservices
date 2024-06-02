using Recipes.Data.DataTransferObjects.UserDTOs;

namespace Recipes.Data.DataTransferObjects;

public class RespondDto
{
    public string Text { get; set; } = string.Empty;
    public int Rate { get; set; }
    public Guid RecipeId { get; set; }
    
    public GetUserDto? User { get; set; } = null!;
}