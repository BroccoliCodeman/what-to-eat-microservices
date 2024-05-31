using Recipes.DAL.Interfaces.ModelsRepositories;

namespace Recipes.DAL.Interfaces;

public interface IUnitOfWork
{
    ICookingStepRepository CookingStepRepository { get; }
    IIngredientRepository IngredientRepository { get; }
    //IRecipeIngredientRepository RecipeIngredientRepository { get; }
    IRecipeRepository RecipeRepository { get; }
    IRespondRepository RespondRepository { get; }
    IWeightUnitRepository WeightUnitRepository { get; }
    
    Task SaveChangesAsync();
}