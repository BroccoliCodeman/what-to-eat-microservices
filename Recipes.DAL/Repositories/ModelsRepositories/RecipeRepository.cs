using Microsoft.EntityFrameworkCore;
using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }

    public override Task<List<Recipe>> GetAsync()
    {
        return _table.Include(p => p.Ingredients).ToListAsync();
    }




}