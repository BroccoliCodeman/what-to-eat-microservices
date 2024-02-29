using Microsoft.EntityFrameworkCore;
using Recipes.DAL.Interfaces;

namespace Recipes.DAL.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly RecipesContext _databaseContext;
    private readonly DbSet<TEntity> _table;

    protected GenericRepository(RecipesContext databaseContext)
    {
        this._databaseContext = databaseContext;
        _table = this._databaseContext.Set<TEntity>();
    }

    public virtual async Task<List<TEntity>> GetAsync() => 
        await _table.ToListAsync();

    public virtual async Task<TEntity> GetByIdAsync(Guid id) => 
        (await _table.FindAsync(id))!;

    public virtual async Task InsertAsync(TEntity entity) => 
        await _table.AddAsync(entity);

    public virtual async Task UpdateAsync(TEntity entity) => 
        await Task.Run(() => _table.Update(entity)).ConfigureAwait(false);

    public virtual async Task DeleteAsync(Guid id) =>
        await Task.Run(async () => _table.Remove(await GetByIdAsync(id)));
}