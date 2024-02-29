using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;

namespace Recipes.BLL.Interfaces;

public interface IRespondService
{
    Task<IBaseResponse<RespondDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<RespondDto>>> Get();
    Task<IBaseResponse<string>> Insert(RespondDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}