namespace Recipes.Data.DataTransferObjects;

public class IngredientDto
{
    public float Quantity { get; set; }
    public string Name { get; set; }

    public WeightUnitDto WeightUnit { get; set; }

}