using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.DAL.Repositories.Interfaces;

namespace Recipes.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly RecipesContext DatabaseContext;

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
        this.DatabaseContext = databaseContext;
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