using Recipes.Data.DataTransferObjects;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface IRespondService
{
    Task<IBaseResponse<RespondDto>> GetById(Guid id);
    Task<IBaseResponse<List<RespondDto>>> Get();
    Task<IBaseResponse<string>> Insert(AddRespondDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
}