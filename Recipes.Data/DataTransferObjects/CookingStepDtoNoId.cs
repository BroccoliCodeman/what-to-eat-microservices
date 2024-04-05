namespace Recipes.Data.DataTransferObjects;

public class CookingStepDtoNoId
{
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    
   // public Guid RecipeId { get; set; }
}