namespace Recipes.Data.DataTransferObjects;

public class RecipeDto
{
    public Guid Id { get; set; }
    public int Servings { get; set; }
    public int CookingTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Calories { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
}