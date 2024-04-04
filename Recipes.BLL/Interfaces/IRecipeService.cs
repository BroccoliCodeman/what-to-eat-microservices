using Recipes.Data.DataTransferObjects;
using Recipes.Data.Interfaces;
using Recipes.Data.Models.HelpersModel;

namespace Recipes.BLL.Interfaces;

public interface IRecipeService
{
    Task<IBaseResponse<RecipeDto>> GetById(Guid id);
    Task<IBaseResponse<IEnumerable<RecipeDto>>> Get(CookingTimeModel? cookingTimeModel);
    Task<IBaseResponse<string>> Insert(RecipeDto? modelDto);
    Task<IBaseResponse<string>> DeleteById(Guid id);
    Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByName(string name);
    Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients);
    Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredients? modelDto);
}