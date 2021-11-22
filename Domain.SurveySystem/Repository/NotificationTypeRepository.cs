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
    public class NotificationTypeRepository : IRepository<NotificationType, Guid>
    {
        private SurveySystemContext db;
        public NotificationTypeRepository(SurveySystemContext context)
        {
            this.db = context;
        }
        public void Create(NotificationType model)
        {
            if (model != null)
                db.NotificationTypes.Add(model);
        }
        public async Task DeleteAsync(Guid id)
        {
            var item = await db.NotificationTypes.FindAsync(id);
            if (item != null)
            {
                db.NotificationTypes.Remove(item);
            }
        }
        public async Task<IEnumerable<NotificationType>> FindAsync(Expression<Func<NotificationType, bool>> predicate)
        {
            return await db.NotificationTypes.OrderByDescending(g => g.CreatedOn).Where(predicate).ToListAsync();
        }
        public async Task<IEnumerable<NotificationType>> GetAllAsync()
        {
            return await db.NotificationTypes.OrderBy(g => g.CreatedOn).AsNoTracking().ToListAsync();
        }
        public async Task<NotificationType> GetAsync(Guid id)
        {
            return await db.NotificationTypes.FindAsync(id);
        }

        public IQueryable<NotificationType> GetIQueryable()
        {
            return db.NotificationTypes.OrderBy(g => g.CreatedOn);
        }
        public async Task<NotificationType> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new NotificationType();
            return await db.NotificationTypes.FirstOrDefaultAsync(x => x.NameRus.ToUpper().Contains(name.Trim().ToUpper()) || x.NameEng.ToUpper().Contains(name.Trim().ToUpper()));
        }
        public void Update(NotificationType model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }
    }
}