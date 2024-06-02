namespace Recipes.Data.DataTransferObjects;

public class AddRespondDto
{
    public string Text { get; set; } = string.Empty;
    public int Rate { get; set; }
    public Guid RecipeId { get; set; }
    public Guid UserId { get; set; }
}