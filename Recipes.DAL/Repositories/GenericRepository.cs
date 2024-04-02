using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Interfaces;

namespace Recipes.DAL.Repositories
{
    public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly RecipesContext _databaseContext;
        protected readonly DbSet<TEntity> _table;
        private readonly ILogger<TEntity> logger;

        public GenericRepository(RecipesContext databaseContext, ILogger<TEntity> logger)
        {
            _databaseContext = databaseContext;
            _table = _databaseContext.Set<TEntity>();
            this.logger = logger;
        }

        public virtual async Task<List<TEntity>> GetAsync()
        {
            logger.LogInformation($"        Getting all {typeof(TEntity).Name} entities");
            return await _table.ToListAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(Guid id)
        {
            logger.LogInformation($"        Getting {typeof(TEntity).Name} entity with id {id}");
            return await _table.FindAsync(id);
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            logger.LogInformation($"        Inserting {typeof(TEntity).Name} entity with id {entity.ToString()}");
            await _table.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            logger.LogInformation($"        Updating {typeof(TEntity).Name} entity with id {entity.ToString()}");
            await Task.Run(() => _databaseContext.Update(entity));
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            logger.LogInformation($"        Deleting {typeof(TEntity).Name} entity with id {id}");
            var entity = await _table.FindAsync(id);

            if (entity == null)
            {
                throw new Exception(GetEntityNotFoundErrorMessage(id));
            }

            _table.Remove(entity);
        }

        protected static string GetEntityNotFoundErrorMessage(Guid id) =>
               $"       {typeof(TEntity).Name} with id {id} not found.";
    }
}
