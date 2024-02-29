namespace Recipes.Data.Models;

public class SavedRecipe
{
    public bool IsSaved { get; set; }
    
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}