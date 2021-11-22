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
   public class UserAnswerRepository : IRepository<UserAnswer, Guid>
    {
        private SurveySystemContext db;
        public UserAnswerRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<UserAnswer> GetIQueryable()
        {
            return db.UserAnswers.Include(x=>x.Invitation).OrderBy(p => p.CreatedOn);
        }

        public async Task<IEnumerable<UserAnswer>> GetAllAsync()
        {
            return await db.UserAnswers.Include(x => x.Invitation).OrderBy(p => p.CreatedOn).AsNoTracking().ToListAsync();
        }
        public async Task<UserAnswer> GetAsync(Guid id)
        {
            return await db.UserAnswers.Include(x => x.Invitation).FirstOrDefaultAsync(x => x.UserAnswerId == id);
        }

        public async Task<UserAnswer> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new UserAnswer();
            return await db.UserAnswers.Include(x => x.Invitation).FirstOrDefaultAsync(x => x.UserAnswerText.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<UserAnswer>> FindAsync(Expression<Func<UserAnswer, Boolean>> predicate)
        {
            return await db.UserAnswers.Include(x => x.Invitation).Where(predicate).OrderBy(p => p.CreatedOn).ToListAsync();
        }

        public void Create(UserAnswer model)
        {
            if (model != null)
                db.UserAnswers.Add(model);
        }

        public void Update(UserAnswer model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.UserAnswers.FindAsync(id);
            if (item != null)
            {
                db.UserAnswers.Remove(item);
            }
        }
    }
}
