using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;

namespace Recipes.BLL.Interfaces;

public interface ICookingStepService
{
    Task<IBaseResponse<CookingStepDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<CookingStepDto>>> Get();
    Task<IBaseResponse<string>> Insert(CookingStepDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}