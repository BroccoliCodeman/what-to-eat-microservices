using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories;

public class RespondRepository : GenericRepository<Respond>, IRespondRepository
{
    public RespondRepository(RecipesContext databaseContext, ILogger<Respond> logger) : base(databaseContext, logger)
    {
    }
}