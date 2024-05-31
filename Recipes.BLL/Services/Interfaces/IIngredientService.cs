using Recipes.Data.DataTransferObjects;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface IIngredientService
{
    Task<IBaseResponse<IngredientDto>> GetById(Guid id);
    Task<IBaseResponse<List<IngredientDto>>> Get();
    Task<IBaseResponse<string>> Insert(IngredientDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}