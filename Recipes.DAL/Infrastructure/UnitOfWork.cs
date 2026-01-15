using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.DAL.Repositories.Interfaces;

namespace Recipes.DAL.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    public RecipesContext DatabaseContext { get; }

    public ICookingStepRepository CookingStepRepository { get; }
    public IIngredientRepository IngredientRepository { get; }
    public IRecipeRepository RecipeRepository { get; }
    public IRespondRepository RespondRepository { get; }
    public IWeightUnitRepository WeightUnitRepository { get; }

    public UnitOfWork(
        RecipesContext databaseContext,
        ICookingStepRepository cookingStepRepository,
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IRespondRepository respondRepository,
        IWeightUnitRepository weightUnitRepository)
    {
        DatabaseContext = databaseContext;
        CookingStepRepository = cookingStepRepository;
        IngredientRepository = ingredientRepository;
        RecipeRepository = recipeRepository;
        RespondRepository = respondRepository;
        WeightUnitRepository = weightUnitRepository;
    }

    public async Task SaveChangesAsync()
    {
        await DatabaseContext.SaveChangesAsync();
    }
}