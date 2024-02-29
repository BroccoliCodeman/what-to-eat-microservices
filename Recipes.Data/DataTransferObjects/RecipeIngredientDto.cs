namespace Recipes.Data.DataTransferObjects;

public class RecipeIngredientDto
{
    public Guid Id { get; set; }
    
    public Guid RecipeId { get; set; }
    public Guid IngredientId { get; set; }
}