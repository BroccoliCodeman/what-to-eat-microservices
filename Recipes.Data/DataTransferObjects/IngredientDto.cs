namespace Recipes.Data.DataTransferObjects;

public class IngredientDto
{
    public Guid Id { get; set; }
    public float Quantity { get; set; }
    
    public Guid WeightUnitId { get; set; }
}