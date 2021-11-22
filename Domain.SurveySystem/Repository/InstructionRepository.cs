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
    public class InstructionRepository : IRepository<Instruction, Guid>
    {
        private SurveySystemContext db;

        public InstructionRepository(SurveySystemContext context)
        {
            this.db = context;
        }

        public void Create(Instruction item)
        {
            if (item != null)
                db.Instructions.Add(item);
        }

        public async Task DeleteAsync(Guid id)
        {
            var instr = await db.Instructions.FindAsync(id);
            if (instr != null)
            {
                db.Instructions.Remove(instr);
            }
        }

        public async Task<IEnumerable<Instruction>> FindAsync(Expression<Func<Instruction, bool>> predicate)
        {
            return await db.Instructions.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<Instruction>> GetAllAsync()
        {
            return await db.Instructions.AsNoTracking().OrderByDescending(x => x.CreatedOn).ToListAsync();
        }

        public async Task<Instruction> GetAsync(Guid id)
        {
            return await db.Instructions.FindAsync(id);
        }

        public IQueryable<Instruction> GetIQueryable()
        {
            return db.Instructions.OrderByDescending(x => x.CreatedOn);
        }

        public async Task<Instruction> GetNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new Instruction();
            return await db.Instructions.FirstOrDefaultAsync(x => x.NameRus.ToUpper().Contains(name.Trim().ToUpper()) ||
                x.NameEng.ToUpper().Contains(name.Trim().ToUpper()));
        }

        public void Update(Instruction item)
        {
            if (item != null)
                db.Entry(item).State = EntityState.Modified;
        }
    }
}