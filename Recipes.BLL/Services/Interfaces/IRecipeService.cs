using Recipes.Data.DataTransferObjects;
using Recipes.Data.Helpers;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface IRecipeService
{
    Task<IBaseResponse<RecipeIntroDto>> GetRandom();
    Task<IBaseResponse<RecipeDto>> GetById(Guid id);
    Task<IBaseResponse<List<RecipeIntroDto>>> GetMostPopularRecipesTitles();
    Task<IBaseResponse<List<RecipeIntroDto>>> GetByTitle(string title);
    Task<IBaseResponse<PagedList<RecipeDto>>> Get(PaginationParams paginationParams, SearchParams? searchParams, int sortType = 0);
    Task<IBaseResponse<string>> Insert(RecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
    Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? modelDto);
    Task<IBaseResponse<string>> SaveRecipe(Guid UserId, Guid RecipeId);
    Task<IBaseResponse<string>> RemoveRecipeFromSaved(Guid UserId, Guid RecipeId);
    Task<IBaseResponse<List<RecipeIntroDto>>> GetByUserId(Guid UserId);
    Task<IBaseResponse<List<RecipeIntroDto>>> GetByUserIdRecipes(Guid UserId);
    Task<IBaseResponse<string>> Update(Guid recipeId, RecipeDto? modelDto);
}