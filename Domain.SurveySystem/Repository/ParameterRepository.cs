using Domain.SurveySystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.SurveySystem.Context;
using Domain.SurveySystem.Entity;

namespace Domain.SurveySystem.Repository
{
   public class ParameterRepository : IRepository<Parameter, Guid>
    {
        private SurveySystemContext db;
        public ParameterRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Parameter> GetIQueryable()
        {
            return db.Parameters.Include(t => t.Criterion).OrderBy(p => p.Criterion.Order).ThenBy(p => p.Order);
        }

        public async Task<IEnumerable<Parameter>> GetAllAsync()
        {
            return await db.Parameters.Include(t => t.Criterion).OrderBy(p => p.Criterion.Order).ThenBy(p=>p.Order).AsNoTracking().ToListAsync();
        }
        public async Task<Parameter> GetAsync(Guid id)
        {
            return await db.Parameters.Include(t => t.Criterion).FirstOrDefaultAsync(x => x.ParameterId == id);
        }

        public async Task<Parameter> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Parameter();
            return await db.Parameters.Include(t => t.Criterion).FirstOrDefaultAsync(x => x.Name.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Parameter>> FindAsync(Expression<Func<Parameter, Boolean>> predicate)
        {
            return await db.Parameters.Include(t => t.Criterion).Where(predicate).OrderBy(p => p.Criterion.Order).ThenBy(p => p.Order).ToListAsync();
        }

        public void Create(Parameter model)
        {
            if (model != null)
                db.Parameters.Add(model);
        }

        public void Update(Parameter model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Parameters.FindAsync(id);
            if (item != null)
            {
                db.Parameters.Remove(item);
            }
        }
    }
}
