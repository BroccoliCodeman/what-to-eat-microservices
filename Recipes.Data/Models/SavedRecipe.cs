namespace Recipes.Data.Models;

public class SavedRecipe
{

    public Guid Id { get; set; }
    public bool IsSaved { get; set; }
    
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public Guid UserId { get; set; }   
}