namespace Recipes.Data.Models;

public class Respond
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Rate { get; set; }
    
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}