using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.SurveySystem.Context;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace Domain.SurveySystem.Repository
{
    public class CriterionRepository : IRepository<Criterion, Guid>
    {
        private SurveySystemContext db;
        public CriterionRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Criterion> GetIQueryable()
        {
            return db.Criterions.OrderBy(p => p.Order);
        }

        public async Task<IEnumerable<Criterion>> GetAllAsync()
        {
            return await db.Criterions.OrderBy(p => p.Order).AsNoTracking().ToListAsync();
        }
        public async Task<Criterion> GetAsync(Guid id)
        {
            return await db.Criterions.FirstOrDefaultAsync(x => x.CriterionId == id);
        }

        public async Task<Criterion> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Criterion();
            return await db.Criterions.FirstOrDefaultAsync(x => x.Name.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Criterion>> FindAsync(Expression<Func<Criterion, Boolean>> predicate)
        {
            return await db.Criterions.Where(predicate).OrderBy(p => p.Order).ToListAsync();
        }

        public void Create(Criterion model)
        {
            if (model != null)
                db.Criterions.Add(model);
        }

        public void Update(Criterion model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Criterions.FindAsync(id);
            if (item != null)
            {
                db.Criterions.Remove(item);
            }
        }
    }
}
