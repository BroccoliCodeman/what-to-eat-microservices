using Microsoft.EntityFrameworkCore;
using Recipes.DAL.Interfaces;

namespace Recipes.DAL.Repositories;

public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly RecipesContext _databaseContext;
    protected readonly DbSet<TEntity> _table;

    public GenericRepository(RecipesContext databaseContext)
    {
        _databaseContext = databaseContext;
        _table = _databaseContext.Set<TEntity>();
    }

    public virtual async Task<List<TEntity>> GetAsync() =>
        await _table.ToListAsync();

    public virtual async Task<TEntity> GetByIdAsync(Guid id) =>
        (await _table.FindAsync(id))!;

    public virtual async Task InsertAsync(TEntity entity)
    {
        await _table.AddAsync(entity);
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        await Task.Run(() => _databaseContext.Update(entity));
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _table.FindAsync(id);

        if (entity == null)
        {
            throw new Exception(
                GetEntityNotFoundErrorMessage(id));
        }
        _table.Remove(entity);
    }

    protected static string GetEntityNotFoundErrorMessage(Guid id) =>
           $"{typeof(TEntity).Name} with id {id} not found.";
}