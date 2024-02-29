namespace Recipes.Data.DataTransferObjects;

public class SavedRecipeDto
{
    public bool IsSaved { get; set; }
    
    public Guid RecipeId { get; set; }
}