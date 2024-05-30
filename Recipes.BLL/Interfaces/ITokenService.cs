using Recipes.Data.Models;
using System.Threading.Tasks;

namespace Recipes.BLL.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}
