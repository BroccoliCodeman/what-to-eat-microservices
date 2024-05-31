using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.DAL.Repositories.Interfaces;

namespace Recipes.DAL.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly RecipesContext _recipesContext;

    public ICookingStepRepository CookingStepRepository { get; }
    public IIngredientRepository IngredientRepository { get; }
    public IRecipeRepository RecipeRepository { get; }
    public IRespondRepository RespondRepository { get; }
    public ISavedRecipeRepository SavedRecipeRepository { get; }
    public IWeightUnitRepository WeightUnitRepository { get; }

    public UnitOfWork(
        RecipesContext recipesContext,
        ICookingStepRepository cookingStepRepository,
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IRespondRepository respondRepository,
        ISavedRecipeRepository savedRecipeRepository,
        IWeightUnitRepository weightUnitRepository)
    {
        _recipesContext = recipesContext;
        CookingStepRepository = cookingStepRepository;
        IngredientRepository = ingredientRepository;
        RecipeRepository = recipeRepository;
        RespondRepository = respondRepository;
        SavedRecipeRepository = savedRecipeRepository;
        WeightUnitRepository = weightUnitRepository;
    }

    public async Task SaveChangesAsync()
    {
        await _recipesContext.SaveChangesAsync();
    }
}