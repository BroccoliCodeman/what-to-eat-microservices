namespace Recipes.Data.Models;

public class Ingredient
{
    public Guid Id { get; set; }
    public float Quantity { get; set; }
    
    public Guid WeightUnitId { get; set; }
    public WeightUnit WeightUnit { get; set; } = null!;
    
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = null!;
}