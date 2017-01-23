using WorkFlowManager.Common.DataAccess.Repositories;
using System;
using System.Collections.Generic;

namespace WorkFlowManager.Common.DataAccess._UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContext _context;

        private Dictionary<string, dynamic> _repositories;

        public UnitOfWork(IDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<string, dynamic>();
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IDbContext GetContext()
        {
            return _context;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IRepository<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(BaseRepository<>);

            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context, this));

            return _repositories[type];
        }

    }
}
