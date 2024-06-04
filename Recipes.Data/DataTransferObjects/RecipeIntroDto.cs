using Recipes.Data.DataTransferObjects.UserDTOs;

namespace Recipes.Data.DataTransferObjects;

public class RecipeIntroDto
{
    public Guid? Id { get; set; } = null;
    public string Title { get; set; } = string.Empty;
    public UserInfo? User { get; set; } = null!;
    public string? Photo { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public int? Calories { get; set; }
    public int SavesCount { get; set; }


}