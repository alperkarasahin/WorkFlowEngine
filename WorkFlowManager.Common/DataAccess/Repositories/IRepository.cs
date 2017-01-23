using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WorkFlowManager.Common.DataAccess.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Get(int id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);


        void Remove(TEntity entity);
        void Remove(int id);
        void RemoveRange(IEnumerable<TEntity> entities);


        TEntity Get(Expression<Func<TEntity, bool>> query = null, params Expression<Func<TEntity, object>>[] includeExpressions);

        TEntity GetForUpdate(Expression<Func<TEntity, bool>> query = null, params Expression<Func<TEntity, object>>[] includeExpressions);



        TEntity Get(Expression<Func<TEntity, bool>> query = null);

        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> query = null, params Expression<Func<TEntity, object>>[] includeExpressions);
        IEnumerable<TEntity> GetListForUpdate(Expression<Func<TEntity, bool>> query = null, params Expression<Func<TEntity, object>>[] includeExpressions);
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> query = null, params Expression<Func<TEntity, object>>[] includeExpressions);

        int GetCount(Expression<Func<TEntity, bool>> query = null);

        void Update(TEntity entity);
        IRepository<T> GetRepository<T>() where T : class;
    }
}