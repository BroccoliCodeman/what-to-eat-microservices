namespace Recipes.Data.Models;

public class Recipe
{
    public Guid Id { get; set; }
    public int Servings { get; set; }
    public int CookingTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Photo { get; set; } 
    public string Description { get; set; } = string.Empty;
    public int Calories { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    
    public ICollection<CookingStep> CookingSteps { get; set; } = null!;
    public ICollection<SavedRecipe> SavedRecipes { get; set; } = null!;
    public ICollection<Respond> Responds { get; set; } = null!;
    
    public ICollection<Ingredient> Ingredients { get; set; } = null!;
}