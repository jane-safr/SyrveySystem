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
    class NotificationRepository : IRepository<Notification, Guid>
    {
        private SurveySystemContext db;
        public NotificationRepository(SurveySystemContext context)
        {
            this.db = context;
        }
        public IQueryable<Notification> GetIQueryable()
        {
            return db.Notifications.Include(t => t.NotificationType).OrderByDescending(g => g.CreatedOn);
        }
        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await db.Notifications.Include(t => t.NotificationType).OrderByDescending(g => g.CreatedOn).AsNoTracking().ToListAsync();
        }
        public async Task<Notification> GetAsync(Guid id)
        {
            return await db.Notifications.Include(t => t.NotificationType).FirstOrDefaultAsync(n => n.NotificationId == id);
        }
        public async Task<Notification> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Notification();
            return await db.Notifications.Include(t => t.NotificationType).FirstOrDefaultAsync(x => x.EmailTo.ToUpper().Contains(name.Trim().ToUpper()));
        }
        public async Task<IEnumerable<Notification>> FindAsync(Expression<Func<Notification, Boolean>> predicate)
        {
            return await db.Notifications.Include(t => t.NotificationType).OrderByDescending(g => g.CreatedOn).Where(predicate).ToListAsync();
        }
        public void Create(Notification model)
        {
            if (model != null)
                db.Notifications.Add(model);
        }
        public void Update(Notification model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var item = await db.Notifications.FindAsync(id);
            if (item != null)
            {
                db.Notifications.Remove(item);
            }
        }
    }
}