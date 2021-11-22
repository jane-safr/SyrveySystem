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
    public class SurveyTypeRepository : IRepository<SurveyType, Guid>
    {
        private SurveySystemContext db;
        public SurveyTypeRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<SurveyType> GetIQueryable()
        {
            return db.SurveyTypes.OrderBy(p => p.NameRus);
        }

        public async Task<IEnumerable<SurveyType>> GetAllAsync()
        {
            return await db.SurveyTypes.OrderBy(p => p.NameRus).AsNoTracking().ToListAsync();
        }
        public async Task<SurveyType> GetAsync(Guid id)
        {
            return await db.SurveyTypes.FirstOrDefaultAsync(x => x.SurveyTypeId == id);
        }

        public async Task<SurveyType> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new SurveyType();
            return await db.SurveyTypes.FirstOrDefaultAsync(x => x.NameRus.ToUpper().Contains(name.Trim().ToUpper()) || x.NameEng.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<SurveyType>> FindAsync(Expression<Func<SurveyType, Boolean>> predicate)
        {
            return await db.SurveyTypes.Where(predicate).OrderBy(p => p.NameRus).ToListAsync();
        }

        public void Create(SurveyType model)
        {
            if (model != null)
                db.SurveyTypes.Add(model);
        }

        public void Update(SurveyType model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.SurveyTypes.FindAsync(id);
            if (item != null)
            {
                db.SurveyTypes.Remove(item);
            }
        }
    }
}