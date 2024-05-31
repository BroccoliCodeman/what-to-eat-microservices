namespace Recipes.Data.Models;

public class Recipe
{
    public Guid Id { get; set; }
    public int Servings { get; set; }
    public int CookingTime { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string? Photo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Calories { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; } = null!;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public ICollection<Ingredient>? Ingredients { get; set; } = null!;
    public ICollection<CookingStep>? CookingSteps { get; set; } = null!;
    public ICollection<User>? Users { get; set; } = null!;
    public ICollection<Respond>? Responds { get; set; } = null!;
}