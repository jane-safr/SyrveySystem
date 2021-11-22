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
    public class QuestionRepository : IRepository<Question, Guid>
    {
        private SurveySystemContext db;
        public QuestionRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Question> GetIQueryable()
        {
           return db.Questions.Include(s => s.Survey).Include(t=>t.QuestionType).Include(p=> p.Answers).Include(i=>i.Indicator).Include(i => i.Indicator.Parameter).Include(i => i.Indicator.Parameter.Criterion).OrderBy(p => p.Group);
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            return await db.Questions.Include(s => s.Survey).Include(t => t.QuestionType).Include(i => i.Indicator).OrderBy(p => p.Group).AsNoTracking().ToListAsync();
        }
        public async Task<Question> GetAsync(Guid id)
        {
            return await db.Questions.Include(s => s.Survey).Include(t => t.QuestionType).Include(i => i.Indicator).Include(i => i.Answers).FirstOrDefaultAsync(x => x.QuestionId == id);
        }

        public async Task<Question> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Question();
            return await db.Questions.Include(s => s.Survey).Include(t => t.QuestionType).Include(i => i.Indicator).Include(i => i.Answers).FirstOrDefaultAsync(x => x.QuestionRus.ToUpper().Contains(name.Trim().ToUpper()) || x.QuestionEng.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Question>> FindAsync(Expression<Func<Question, Boolean>> predicate)
        {
            return await db.Questions.Include(t => t.QuestionType).Include(i => i.Answers).Include(i => i.Indicator).Where(predicate).OrderBy(p => p.Group).ToListAsync();
        }

        public void Create(Question model)
        {
            if (model != null)
                db.Questions.Add(model);
        }

        public void Update(Question model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Questions.FindAsync(id);
            if (item != null)
            {
                db.Questions.Remove(item);
            }
        }
    }
}
