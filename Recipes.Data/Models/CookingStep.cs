namespace Recipes.Data.Models;

public class CookingStep
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid? RecipeId { get; set; }
    public Recipe? Recipe { get; set; } 
}