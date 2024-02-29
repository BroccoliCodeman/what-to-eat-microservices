using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;

namespace Recipes.BLL.Interfaces;

public interface ISavedRecipeService
{
    Task<IBaseResponse<SavedRecipeDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<SavedRecipeDto>>> Get();
    Task<IBaseResponse<string>> Insert(SavedRecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}