using Recipes.Data.DataTransferObjects.UserDTOs;

namespace Recipes.Data.DataTransferObjects;

public class RespondDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Rate { get; set; } = 0;
    public GetUserDto? User { get; set; } = null!;
}