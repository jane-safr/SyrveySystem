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
    class InvitationRepository : IRepository<Invitation, Guid>
    {
        private SurveySystemContext db;
        public InvitationRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public IQueryable<Invitation> GetIQueryable()
        {
            return db.Invitations.Include(s => s.Survey).OrderByDescending(p => p.DateEnd);
        }

        public async Task<IEnumerable<Invitation>> GetAllAsync()
        {
            return await db.Invitations.Include(s=>s.Survey).OrderByDescending(p => p.DateEnd).AsNoTracking().ToListAsync();
        }
        public async Task<Invitation> GetAsync(Guid id)
        {
            return await db.Invitations.Include(s => s.Survey).FirstOrDefaultAsync(x => x.InvitationId == id);
        }

        public async Task<Invitation> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Invitation();
            return await db.Invitations.Include(s => s.Survey).FirstOrDefaultAsync(x => x.UserName.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public async Task<IEnumerable<Invitation>> FindAsync(Expression<Func<Invitation, Boolean>> predicate)
        {
            return await db.Invitations.Include(s => s.Survey).Where(predicate).OrderByDescending(p => p.DateEnd).ToListAsync();
        }

        public void Create(Invitation model)
        {
            if (model != null)
                db.Invitations.Add(model);
        }

        public void Update(Invitation model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await db.Invitations.FindAsync(id);
            if (item != null)
            {
                db.Invitations.Remove(item);
            }
        }
    }
}
