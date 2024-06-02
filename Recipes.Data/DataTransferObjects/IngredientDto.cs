namespace Recipes.Data.DataTransferObjects;

public class IngredientDto
{
    public float Quantity { get; set; }
    public IngredientTypeDto IngredientType { get; set; }
    public WeightUnitDto WeightUnit { get; set; }

}