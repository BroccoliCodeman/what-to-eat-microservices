using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class RespondRepository : GenericRepository<Respond>, IRespondRepository
{
    public RespondRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}