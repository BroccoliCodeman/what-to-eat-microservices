using Recipes.Data.DataTransferObjects;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services.Interfaces;

public interface IRecipeService
{
    Task<IBaseResponse<RecipeDto>> GetById(Guid id);
    Task<IBaseResponse<List<RecipeDto>>> Get();
    Task<IBaseResponse<string>> Insert(RecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
    Task<IBaseResponse<List<RecipeDto>>> GetByName(string name);
    Task<IBaseResponse<List<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients);
    Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? modelDto);
}