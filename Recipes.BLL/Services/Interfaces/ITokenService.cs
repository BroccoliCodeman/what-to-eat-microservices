using Recipes.Data.Models;

namespace Recipes.BLL.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}
