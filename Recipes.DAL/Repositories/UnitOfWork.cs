using Recipes.DAL.Interfaces;
using Recipes.DAL.Interfaces.ModelsRepositories;

namespace Recipes.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    protected readonly RecipesContext DatabaseContext;

    public ICookingStepRepository CookingStepRepository { get; }
    public IIngredientRepository IngredientRepository { get; }
  //  public IRecipeIngredientRepository RecipeIngredientRepository { get; }
    public IRecipeRepository RecipeRepository { get; }
    public IRespondRepository RespondRepository { get; }
    public ISavedRecipeRepository SavedRecipeRepository { get; }
    public IWeightUnitRepository WeightUnitRepository { get; }

    public UnitOfWork(
        RecipesContext databaseContext,
        ICookingStepRepository cookingStepRepository,
        IIngredientRepository ingredientRepository,
       // IRecipeIngredientRepository recipeIngredientRepository,
        IRecipeRepository recipeRepository,
        IRespondRepository respondRepository,
        ISavedRecipeRepository savedRecipeRepository,
        IWeightUnitRepository weightUnitRepository)
    {
        this.DatabaseContext = databaseContext;
        CookingStepRepository = cookingStepRepository;
        IngredientRepository = ingredientRepository;
    //    RecipeIngredientRepository = recipeIngredientRepository;
        RecipeRepository = recipeRepository;
        RespondRepository = respondRepository;
        SavedRecipeRepository = savedRecipeRepository;
        WeightUnitRepository = weightUnitRepository;
    }

    public async Task SaveChangesAsync()
    {
        var saved = await DatabaseContext.SaveChangesAsync();
    }
}