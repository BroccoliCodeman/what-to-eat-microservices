using Recipes.Data.DataTransferObjects;
using Recipes.Data.Helpers;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface IRecipeService
{
    Task<IBaseResponse<RecipeDto>> GetById(Guid id);
    Task<IBaseResponse<PagedList<RecipeDto>>> Get(PaginationParams paginationParams, SearchParams? searchParams);
    Task<IBaseResponse<string>> Insert(RecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
/*    Task<IBaseResponse<PagedList<RecipeDto>>> GetByName(string name);
    Task<IBaseResponse<PagedList<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients);*/
    Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? modelDto);
}