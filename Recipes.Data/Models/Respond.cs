namespace Recipes.Data.Models;

public class Respond
{       
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Rate { get; set; } = 0;
    
    public Guid? RecipeId { get; set; }
    public Recipe? Recipe { get; set; } = null!; 
    public Guid? UserId { get; set; }
    public User? User { get; set; } = null!;

}