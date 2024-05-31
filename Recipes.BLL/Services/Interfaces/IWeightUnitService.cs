using Recipes.Data.DataTransferObjects;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface IWeightUnitService
{
    Task<IBaseResponse<WeightUnitDto>> GetById(Guid id);
    Task<IBaseResponse<List<WeightUnitDto>>> Get();
    Task<IBaseResponse<string>> Insert(WeightUnitDto? modelDto);
}