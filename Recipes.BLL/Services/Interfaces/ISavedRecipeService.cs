using Recipes.Data.DataTransferObjects;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface ISavedRecipeService
{
    Task<IBaseResponse<SavedRecipeDto>> GetById(Guid id);
    Task<IBaseResponse<List<SavedRecipeDto>>> Get();
    Task<IBaseResponse<string>> Insert(SavedRecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}