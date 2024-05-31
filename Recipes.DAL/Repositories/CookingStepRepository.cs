using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories;

public class CookingStepRepository : GenericRepository<CookingStep>, ICookingStepRepository
{
    public CookingStepRepository(RecipesContext databaseContext,ILogger<CookingStep> logger) : base(databaseContext,logger)
    {
    }
}