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
    public class IndicatorRepository : IRepository<Indicator, Guid>
    {
        private SurveySystemContext db;
        public IndicatorRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Indicator> GetIQueryable()
        {
            return db.Indicators.Include(t => t.Parameter).Include(x => x.Parameter.Criterion).OrderBy(p => p.Parameter.Criterion.Order).ThenBy(p => p.Parameter.Order).ThenBy(o => o.Order);
        }

        public async Task<IEnumerable<Indicator>> GetAllAsync()
        {
            return await db.Indicators.Include(t => t.Parameter).Include(x=>x.Parameter.Criterion).OrderBy(p => p.Order).AsNoTracking().ToListAsync();
        }
        public async Task<Indicator> GetAsync(Guid id)
        {
            return await db.Indicators.Include(t => t.Parameter).Include(x => x.Parameter.Criterion).FirstOrDefaultAsync(x => x.IndicatorId == id);
        }

        public async Task<Indicator> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Indicator();
            return await db.Indicators.Include(t => t.Parameter).Include(x => x.Parameter.Criterion).FirstOrDefaultAsync(x => x.Name.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Indicator>> FindAsync(Expression<Func<Indicator, Boolean>> predicate)
        {
            return await db.Indicators.Include(t => t.Parameter).Include(x => x.Parameter.Criterion).Where(predicate).OrderBy(p => p.Parameter.Criterion.Order).ThenBy(p=>p.Parameter.Order).ThenBy(o=>o.Order).ToListAsync();
        }

        public void Create(Indicator model)
        {
            if (model != null)
                db.Indicators.Add(model);
        }

        public void Update(Indicator model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }
        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Indicators.FindAsync(id);
            if (item != null)
            {
                db.Indicators.Remove(item);
            }
        }
    }
}