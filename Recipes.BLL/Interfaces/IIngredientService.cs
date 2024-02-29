using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;

namespace Recipes.BLL.Interfaces;

public interface IIngredientService
{
    Task<IBaseResponse<IngredientDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<IngredientDto>>> Get();
    Task<IBaseResponse<string>> Insert(IngredientDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}