namespace Recipes.Data.Models;

public class SavedRecipe
{
    public SavedRecipe()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; set; }
    public bool IsSaved { get; set; }
    
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}