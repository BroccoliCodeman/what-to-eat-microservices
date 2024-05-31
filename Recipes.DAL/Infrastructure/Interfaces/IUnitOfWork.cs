using Recipes.DAL.Repositories.Interfaces;

namespace Recipes.DAL.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    ICookingStepRepository CookingStepRepository { get; }
    IIngredientRepository IngredientRepository { get; }
    //IRecipeIngredientRepository RecipeIngredientRepository { get; }
    IRecipeRepository RecipeRepository { get; }
    IRespondRepository RespondRepository { get; }
    ISavedRecipeRepository SavedRecipeRepository { get; }
    IWeightUnitRepository WeightUnitRepository { get; }
    
    Task SaveChangesAsync();
}