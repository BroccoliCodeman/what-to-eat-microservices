using Recipes.Data.DataTransferObjects;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface ICookingStepService
{
    Task<IBaseResponse<CookingStepDto>> GetById(Guid id);
    Task<IBaseResponse<List<CookingStepDto>>> Get();
    Task<IBaseResponse<string>> Insert(CookingStepDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}