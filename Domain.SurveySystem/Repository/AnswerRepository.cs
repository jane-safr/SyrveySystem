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
    public class AnswerRepository : IRepository<Answer, Guid>
    {
        private SurveySystemContext db;
        public AnswerRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Answer> GetIQueryable()
        {
            return db.Answers.Include(q=>q.Question).OrderByDescending(p => p.Credit);
        }

        public async Task<IEnumerable<Answer>> GetAllAsync()
        {
            return await db.Answers.Include(q => q.Question).OrderByDescending(p => p.Credit).AsNoTracking().ToListAsync();
        }
        public async Task<Answer> GetAsync(Guid id)
        {
            return await db.Answers.Include(q => q.Question).FirstOrDefaultAsync(x => x.AnswerId == id);
        }

        public async Task<Answer> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Answer();
            return await db.Answers.Include(q => q.Question).FirstOrDefaultAsync(x => x.AnswerRus.ToUpper().Contains(name.Trim().ToUpper()) || x.AnswerEng.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Answer>> FindAsync(Expression<Func<Answer, Boolean>> predicate)
        {
            return await db.Answers.Include(q => q.Question).Where(predicate).OrderByDescending(p => p.Credit).ToListAsync();
        }

        public void Create(Answer model)
        {
            if (model != null)
                db.Answers.Add(model);
        }
        public void Update(Answer model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }
        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Answers.FindAsync(id);
            if (item != null)
            {
                db.Answers.Remove(item);
            }
        }
    }
}
