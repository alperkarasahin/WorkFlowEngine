using WorkFlowManager.Common.DataAccess._UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WorkFlowManager.Common.DataAccess.Repositories
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        protected readonly IDbContext _context;
        private readonly IDbSet<TEntity> _table;
        private readonly IUnitOfWork _unitOfWork;

        public BaseRepository(IDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _table = _context.Set<TEntity>();
        }

        public TEntity Get(int id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.Run(() => Find(predicate));
        }

        public async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> query = null, params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            return await Task.Run(() => GetList(query, includeExpressions));
        }

        public TEntity Get(Expression<Func<TEntity, bool>> query = null,
            params Expression<Func<TEntity, object>>[] includeExpressions)
        {

            IQueryable<TEntity> newquery = _table.AsQueryable();

            if (includeExpressions.Any())
                newquery = includeExpressions.Aggregate(newquery,
                    (current, includeExpression) => current.Include(includeExpression));

            if (query != null)
                newquery = newquery.Where(query);

            TEntity entity = newquery.AsNoTracking().SingleOrDefault();

            return entity;
        }

        public TEntity GetForUpdate(Expression<Func<TEntity, bool>> query = null,
            params Expression<Func<TEntity, object>>[] includeExpressions)
        {

            IQueryable<TEntity> newquery = _table.AsQueryable();

            if (includeExpressions.Any())
                newquery = includeExpressions.Aggregate(newquery,
                    (current, includeExpression) => current.Include(includeExpression));

            if (query != null)
                newquery = newquery.Where(query);

            TEntity entity = newquery.SingleOrDefault();

            return entity;
        }

        public TEntity Get(Expression<Func<TEntity, bool>> query = null)
        {

            IQueryable<TEntity> newquery = _table.AsQueryable();

            if (query != null)
                newquery = newquery.Where(query);

            TEntity entity = newquery.SingleOrDefault();

            return entity;
        }

        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> query = null,
             params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            IQueryable<TEntity> newquery = _table.AsQueryable();

            if (includeExpressions.Any())
                newquery = includeExpressions.Aggregate(newquery,
                    (current, includeExpression) => current.Include(includeExpression));

            if (query != null)
                newquery = newquery.Where(query);

            return newquery.AsNoTracking().ToList();
        }

        public IEnumerable<TEntity> GetListForUpdate(Expression<Func<TEntity, bool>> query = null,
             params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            IQueryable<TEntity> newquery = _table.AsQueryable();

            if (includeExpressions.Any())
                newquery = includeExpressions.Aggregate(newquery,
                    (current, includeExpression) => current.Include(includeExpression));

            if (query != null)
                newquery = newquery.Where(query);

            return newquery.ToList();
        }



        public int GetCount(Expression<Func<TEntity, bool>> query = null)
        {
            if (query == null)
                return _table.Count();
            else
                return _table.Count(query);
        }


        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            _context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException();
            }
            //_context.Set<TEntity>().AddRange(entities);
        }

        public void Remove(int id)
        {
            TEntity entity = Get(id);

            if (entity == null)
            {
                throw new NullReferenceException("Entity is null/Not in collection");
            }
            Remove(entity);
        }

        public void Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            _context.Set<TEntity>().Remove(entity);
        }


        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException();
            }
            foreach (var entity in entities.ToList())
            {
                Remove(entity);
            }

            //(_context.Set<TEntity>() as DbSet).RemoveRange(entities);
        }

        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            _context.Entry(entity).State = EntityState.Modified;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return _unitOfWork.Repository<T>();
        }
    }
}