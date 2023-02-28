using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> AsQueryable();
        IEnumerable<T> AsEnumerable();

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        T GetById(Guid id);
        T Single(Expression<Func<T, bool>> predicate);
        T SingleOrDefault(Expression<Func<T, bool>> predicate);
        T First(Expression<Func<T, bool>> predicate);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        int Count(Expression<Func<T, bool>> predicate);
        DbQuery<T> Include(string path);

        void Add(T entity);
        void Delete(T entity);
        void Update(T entity, bool is_anonymous = false, bool is_time_change = true);
        void HardDelete(T entity);
        void HardDeleteRange(IQueryable<T> need_remove);
        bool HasChanges();
        string GetUserName();
    }
}
