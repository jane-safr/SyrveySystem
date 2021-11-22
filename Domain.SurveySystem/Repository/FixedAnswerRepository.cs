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
    class FixedAnswerRepository : IRepository<FixedAnswer, Guid>
    {
        private SurveySystemContext db;
        public FixedAnswerRepository(SurveySystemContext context)
        {
            this.db = context;
        }
        public IQueryable<FixedAnswer> GetIQueryable()
        {
            return db.FixedAnswers.Include(t => t.QuestionType).OrderBy(g => g.Credit);
        }
        public async Task<IEnumerable<FixedAnswer>> GetAllAsync()
        {
            return await db.FixedAnswers.Include(t => t.QuestionType).OrderBy(g => g.Credit).AsNoTracking().ToListAsync();
        }
        public async Task<FixedAnswer> GetAsync(Guid id)
        {
            return await db.FixedAnswers.Include(t => t.QuestionType).FirstOrDefaultAsync(n => n.FixedAnswerId == id);
        }
        public async Task<FixedAnswer> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new FixedAnswer();
            return await db.FixedAnswers.Include(t => t.QuestionType).FirstOrDefaultAsync(x => x.FixAnswerRus.ToUpper().Contains(name.Trim().ToUpper()) || x.FixAnswerEng.ToUpper().Contains(name.Trim().ToUpper()));
        }
        public async Task<IEnumerable<FixedAnswer>> FindAsync(Expression<Func<FixedAnswer, Boolean>> predicate)
        {
            return await db.FixedAnswers.Include(t => t.QuestionType).OrderBy(g => g.Credit).Where(predicate).ToListAsync();
        }
        public void Create(FixedAnswer model)
        {
            if (model != null)
                db.FixedAnswers.Add(model);
        }
        public void Update(FixedAnswer model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var item = await db.FixedAnswers.FindAsync(id);
            if (item != null)
            {
                db.FixedAnswers.Remove(item);
            }
        }
    }
}