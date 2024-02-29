namespace Recipes.Data.Models;

public class WeightUnit
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    
    public ICollection<Ingredient> Ingredients { get; set; } = null!;
}