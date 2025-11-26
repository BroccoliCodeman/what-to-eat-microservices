namespace Recipes.Data.Models;

public class WeightUnit
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public List<Ingredient>? Ingredients { get; set; } = new List<Ingredient>();
}