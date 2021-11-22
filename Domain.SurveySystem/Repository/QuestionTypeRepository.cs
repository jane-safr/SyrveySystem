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
    public class QuestionTypeRepository : IRepository<QuestionType, Guid>
    {
        private SurveySystemContext db;
        public QuestionTypeRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<QuestionType> GetIQueryable()
        {
            return db.QuestionTypes.Include(a => a.FixedAnswers).OrderBy(p => p.CreatedOn);
        }

        public async Task<IEnumerable<QuestionType>> GetAllAsync()
        {
            return await db.QuestionTypes.Include(a=>a.FixedAnswers).OrderBy(p => p.CreatedOn).AsNoTracking().ToListAsync();
        }
        public async Task<QuestionType> GetAsync(Guid id)
        {
            return await db.QuestionTypes.Include(a => a.FixedAnswers).FirstOrDefaultAsync(x => x.QuestionTypeId == id);
        }

        public async Task<QuestionType> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new QuestionType();
            return await db.QuestionTypes.Include(a => a.FixedAnswers).FirstOrDefaultAsync(x => x.TypeName.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<QuestionType>> FindAsync(Expression<Func<QuestionType, Boolean>> predicate)
        {
            return await db.QuestionTypes.Include(a => a.FixedAnswers).Where(predicate).OrderBy(p => p.CreatedOn).ToListAsync();
        }

        public void Create(QuestionType model)
        {
            if (model != null)
                db.QuestionTypes.Add(model);
        }

        public void Update(QuestionType model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.QuestionTypes.FindAsync(id);
            if (item != null)
            {
                db.QuestionTypes.Remove(item);
            }
        }
    }
}
