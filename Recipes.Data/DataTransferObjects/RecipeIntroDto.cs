namespace Recipes.Data.DataTransferObjects;

public class RecipeIntroDto
{
    public Guid? Id { get; set; } = null;
    public string Title { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}