using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.SurveySystem.Interfaces
{
    public interface IRepository<T, U> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(U id);
        Task<T> GetNameAsync(string name);
        IQueryable<T> GetIQueryable();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        void Create(T item);
        void Update(T item);
        Task DeleteAsync(U id);
    }
}