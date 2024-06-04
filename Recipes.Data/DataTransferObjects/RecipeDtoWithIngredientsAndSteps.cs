using Recipes.Data.Models;

namespace Recipes.Data.DataTransferObjects;

public class RecipeDtoWithIngredientsAndSteps
{
    public Guid? UserId { get; set; } = null;
    public int? Servings { get; set; }
    public int? CookingTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Photo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? Calories { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;

    public List<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    public List<CookingStepDtoNoId> Steps { get; set; } = new List<CookingStepDtoNoId>();




}