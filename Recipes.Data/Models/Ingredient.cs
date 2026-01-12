namespace Recipes.Data.Models;

public class Ingredient
{
    public Guid Id { get; set; }
    public float Quantity { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? WeightUnitId { get; set; }
    public WeightUnit? WeightUnit { get; set; } 
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}