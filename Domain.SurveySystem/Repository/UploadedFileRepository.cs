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
    class UploadedFileRepository : IRepository<UploadedFile, Guid>
    {
        private SurveySystemContext db;
        public UploadedFileRepository(SurveySystemContext context)
        {
            this.db = context;
        }
        public IQueryable<UploadedFile> GetIQueryable()
        {
            return db.UploadedFiles.OrderByDescending(x => x.CreatedOn);
        }
        public async Task<IEnumerable<UploadedFile>> GetAllAsync()
        {
            var res = await db.UploadedFiles.OrderByDescending(x => x.CreatedOn).AsNoTracking().ToListAsync();
            return res;
        }
        public async Task<UploadedFile> GetAsync(Guid id)
        {
            return await db.UploadedFiles.FirstOrDefaultAsync(x => x.UploadedFileId == id);
        }
        public async Task<IEnumerable<UploadedFile>> FindAsync(Expression<Func<UploadedFile, Boolean>> predicate)
        {
            return await db.UploadedFiles.Where(predicate).OrderByDescending(x => x.CreatedOn).ToListAsync();
        }
        public void Create(UploadedFile model)
        {
            if (model != null)
                db.UploadedFiles.Add(model);
        }
        public void Update(UploadedFile model)
        {
            if (model != null)
                db.Entry(model).State = EntityState.Modified;
        }
        public async Task DeleteAsync(Guid id)
        {
            var data = await db.UploadedFiles.FirstOrDefaultAsync(x => x.UploadedFileId == id);
            if (data != null)
            {
                db.UploadedFiles.Remove(data);
            }
        }
        public Task DeleteAsync(string name)
        {
            return Task.CompletedTask;
        }
        public async Task<UploadedFile> GetNameAsync(string name)
        {
            return await Task.FromResult<UploadedFile>(null);
        }
    }
}
