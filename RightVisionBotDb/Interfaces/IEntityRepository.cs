using System.Linq.Expressions;

namespace RightVisionBotDb.Interfaces
{
    public interface IEntityRepository<TEntity> where TEntity : class
    {
        Task AddAsync(TEntity entity, CancellationToken token);
        Task UpdateAsync(TEntity entity, CancellationToken token);
        Task DeleteAsync(TEntity entity, CancellationToken token);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, CancellationToken token);
        Task<T> GetAsync<T>(Expression<Func<T, bool>> expression, CancellationToken token);
        Task<IQueryable<TEntity>> GetAllAsync(CancellationToken token);
        Task<IQueryable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> expression, CancellationToken token);
        Task<int> CountAsync(CancellationToken token);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> expression, CancellationToken token);

    }
}
