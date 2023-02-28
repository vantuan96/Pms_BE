using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace DataAccess.Repository
{
    public class EfGenericRepository<T> : IGenericRepository<T> where T : class
    {
        internal PMSContext _context;
        internal DbSet<T> _dbSet;

        public EfGenericRepository(PMSContext context)
        {
            this._context = context;
            this._dbSet = context.Set<T>();
        }

        public IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public IEnumerable<T> AsEnumerable()
        {
            return _dbSet;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public T GetById(Guid id)
        {
            return _dbSet.Find(id);
        }

        public T Single(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Single(predicate);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.SingleOrDefault(predicate);
        }

        public T First(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.First(predicate);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }

        public DbQuery<T> Include(string path)
        {
            return _dbSet.Include(path);
        }

        public void Add(T entity)
        {
            if(entity is IGuidEntity)
                if (((IGuidEntity)entity).Id == Guid.Empty)
                    ((IGuidEntity)entity).Id = Guid.NewGuid();

            var userName = GetUserName();
            if (entity is ICreateEntity)
            {
                
                ((ICreateEntity)entity).CreatedBy = userName;
                ((ICreateEntity)entity).CreatedAt = DateTime.Now;
            }
            
            if(entity is IUpdatEntity)
            {
                ((IUpdatEntity)entity).UpdatedBy = userName;
                ((IUpdatEntity)entity).UpdatedAt = DateTime.Now;
            }

            if(entity is IDeleteEntity)
                ((IDeleteEntity)entity).IsDeleted = false;

            _dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            var userName = GetUserName();
            if (entity is IUpdatEntity)
            {
                ((IUpdatEntity)entity).UpdatedBy = userName;
                ((IUpdatEntity)entity).UpdatedAt = DateTime.Now;
            }

            if (entity is IDeleteEntity)
            {
                ((IDeleteEntity)entity).IsDeleted = true;
                ((IDeleteEntity)entity).DeletedBy = userName;
                ((IDeleteEntity)entity).DeletedAt = DateTime.Now;
            }
            else
                _dbSet.Remove(entity);
        }

        public void HardDelete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void HardDeleteRange(IQueryable<T> need_remove)
        {
            _dbSet.RemoveRange(need_remove);
        }

        public void Update(T entity, bool is_anonymous = false, bool is_time_change = true)
        {
            if (!(entity is IUpdatEntity))
                return;

            if (!is_anonymous)
            {
                var userName = GetUserName();
                ((IUpdatEntity)entity).UpdatedBy = userName;
            }

            if (is_time_change)
                ((IUpdatEntity)entity).UpdatedAt = DateTime.Now;
        }
        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
        public string GetUserName()
        {
            try
            {
                var claims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();
                return claims?.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
