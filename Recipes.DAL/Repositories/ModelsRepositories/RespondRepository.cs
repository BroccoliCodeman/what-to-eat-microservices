using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class RespondRepository : GenericRepository<Respond>, IRespondRepository
{
    protected RespondRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}