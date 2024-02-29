using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;

namespace Recipes.BLL.Interfaces;

public interface IWeightUnitService
{
    Task<IBaseResponse<WeightUnitDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<WeightUnitDto>>> Get();
    Task<IBaseResponse<string>> Insert(WeightUnitDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}