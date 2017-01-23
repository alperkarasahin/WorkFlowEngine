using WorkFlowManager.Common.DataAccess.Repositories;
using System;

namespace WorkFlowManager.Common.DataAccess._UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        int Complete();
        IDbContext GetContext();
    }
}
