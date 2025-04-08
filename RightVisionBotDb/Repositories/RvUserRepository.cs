using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using System.Linq.Expressions;

namespace RightVisionBotDb.Repositories
{
    internal class RvUserRepository : IEntityRepository<RvUser>
    {
        public Task AddAsync(RvUser entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<RvUser, bool>> expression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(RvUser entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<RvUser>> GetAllAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<RvUser>> GetAllAsync(Expression<Func<RvUser, bool>> expression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<RvUser> GetAsync(Expression<Func<RvUser, bool>> expression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(Expression<Func<T, bool>> expression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(RvUser entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
