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
    public class SettingRepository : IRepository<Setting, Guid>
    {
        private SurveySystemContext db;
        public SettingRepository(SurveySystemContext context)
        {
            this.db = context;
        }
        public void Create(Setting item)
        {
            if (item != null)
                db.Settings.Add(item);
        }
        public void Update(Setting item)
        {
            if (item != null)
                db.Entry(item).State = EntityState.Modified;
        }
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var sett = await db.Settings.FindAsync(id);
            if (sett != null)
            {
                db.Settings.Remove(sett);
            }
        }
        public async Task<IEnumerable<Setting>> GetAllAsync()
        {
            return await db.Settings.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        }
        public async Task<Setting> GetAsync(Guid id)
        {
            return await db.Settings.FindAsync(id);
        }
        public async Task<Setting> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Setting();
            return await db.Settings.FirstOrDefaultAsync(x => x.Name.ToUpper() == name.Trim().ToUpper());
        }
        public async Task<IEnumerable<Setting>> FindAsync(Expression<Func<Setting, bool>> predicate)
        {
            return await db.Settings.Where(predicate).ToListAsync();
        }
        public IQueryable<Setting> GetIQueryable()
        {
            return db.Settings.OrderBy(x => x.Name);
        }
    }
}