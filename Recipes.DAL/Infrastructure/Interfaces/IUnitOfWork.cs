using Recipes.DAL.Repositories.Interfaces;

namespace Recipes.DAL.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    ICookingStepRepository CookingStepRepository { get; }
    IIngredientRepository IngredientRepository { get; }
    IRecipeRepository RecipeRepository { get; }
    IRespondRepository RespondRepository { get; }
    IWeightUnitRepository WeightUnitRepository { get; }
    RecipesContext DatabaseContext { get; }

    Task SaveChangesAsync();
}