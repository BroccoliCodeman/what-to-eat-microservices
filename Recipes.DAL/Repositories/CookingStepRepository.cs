using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;
using System.Security.Cryptography.X509Certificates;

namespace Recipes.DAL.Repositories;

public class CookingStepRepository : GenericRepository<CookingStep>, ICookingStepRepository
{
    public CookingStepRepository(RecipesContext databaseContext, ILogger<CookingStep> logger) : base(databaseContext, logger)
    {
    }
    public async override Task<List<CookingStep>> GetAsync()
    {
        return await _table.Include(p=>p.Recipe).ToListAsync();
    }
}
