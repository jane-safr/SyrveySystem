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
    public class SurveyRepository : IRepository<Survey, Guid>
    {
        private SurveySystemContext db;
        public SurveyRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Survey> GetIQueryable()
        {
            return db.Surveys.Include(t=>t.SurveyType).OrderByDescending(p => p.CreatedOn);
        }

        public async Task<IEnumerable<Survey>> GetAllAsync()
        {
            return await db.Surveys.Include(t => t.SurveyType).OrderBy(p => p.NameRus).AsNoTracking().ToListAsync();
        }
        public async Task<Survey> GetAsync(Guid id)
        {
            return await db.Surveys.Include(t => t.SurveyType).Include(i=>i.Invitations).Include(q=>q.Questions).FirstOrDefaultAsync(x => x.SurveyId == id);
        }

        public async Task<Survey> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Survey();
            return await db.Surveys.Include(t => t.SurveyType).FirstOrDefaultAsync(x => x.NameRus.ToUpper().Contains(name.Trim().ToUpper()) || x.NameEng.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Survey>> FindAsync(Expression<Func<Survey, Boolean>> predicate)
        {
            return await db.Surveys.Include(t => t.SurveyType).Include(i=>i.Invitations).Where(predicate).OrderBy(p => p.NameRus).ToListAsync();
        }

        public void Create(Survey model)
        {
            if (model != null)
                db.Surveys.Add(model);
        }

        public void Update(Survey model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Surveys.FindAsync(id);
            if (item != null)
            {
                db.Surveys.Remove(item);
            }
        }
    }
}
