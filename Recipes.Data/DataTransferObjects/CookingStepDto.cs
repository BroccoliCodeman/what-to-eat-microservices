namespace Recipes.Data.DataTransferObjects;

public class CookingStepDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    
    public Guid RecipeId { get; set; }
}