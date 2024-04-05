using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;

namespace Recipes.BLL.Interfaces;

public interface IRecipeService
{
    Task<IBaseResponse<RecipeDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<RecipeDto>>> Get();
    Task<IBaseResponse<string>> Insert(RecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
    Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByName(string name);
    Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients);
    Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? modelDto);
}