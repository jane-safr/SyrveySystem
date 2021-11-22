using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class CriterionService : ICriterionService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<CriterionService> loggingService;
        public CriterionService(IUnitOfWork uow, ILoggerService<CriterionService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<CriterionDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.Criterions.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Criterion>, List<CriterionDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<CriterionDTO>();
            }
        }

        public async Task<IEnumerable<CriterionDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Criterions.GetIQueryable();
                if (filterModels != null && filterModels.Any())
                {
                    foreach (var filter in filterModels)
                    {
                        if (filter != null)
                        {
                            if (filter.Field.StartsWith("IsActive", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    inquiry = inquiry.Where(x => x.IsActive == val);
                                }
                            }
                            else if (filter.Field.StartsWith("Searchtxt", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                inquiry = inquiry.Where(x => x.Name.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.CriterionId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Criterion>, List<CriterionDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<CriterionDTO>();
            }
        }

        public async Task<CriterionDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new CriterionDTO();
                }
                var dataFirst = await Database.Criterions.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<Criterion, CriterionDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new CriterionDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(CriterionDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("CriterionDTO is Empty");
                    return new OperationDetails(false, "Критерий не найден / Criterion is empty", string.Empty);
                }
                if (model.CriterionId == Guid.Empty)
                {
                    loggingService.Error("CriterionId is empty");
                    return new OperationDetails(false, "Id критерия отсутствует / CriterionId is empty", string.Empty);
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    loggingService.Error("Criterion is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                
                
                var criterion = await Database.Criterions.GetAsync(model.CriterionId);
                if (criterion == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }
                // duplicate by Name
                var duplName = await Database.Criterions.FindAsync(x => x.Name == model.Name.Trim()  && x.CriterionId != model.CriterionId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Тип с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                if (model.Order < 1)
                {
                    return new OperationDetails(false, "Номер критерия недействителен / Criterion Code is invalid", "Duplicate");
                }
                // dupl Order
                var duplOrder = await Database.Criterions.FindAsync(x => x.Order == model.Order && x.CriterionId != model.CriterionId);
                if (duplOrder.Any())
                {
                    loggingService.Error($"Duplicate in Code = {model.Order}");
                    return new OperationDetails(false, "Критерий с таким номером уже существует / Duplicate by Code", "Duplicate");
                }
                loggingService.Warn($"Update: NameOld:{criterion.Name} -> NameNew:{model.Name}");
                //Update
                criterion.Name = model.Name.Trim();
                criterion.IsActive = model.IsActive;
                criterion.Order = model.Order;

                Database.Criterions.Update(criterion);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateCriterion");
                }
                else
                {
                    loggingService.Error("UpdateCriterion error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateCriterion");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateCriterion");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty CriterionId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.Criterions.GetAsync(id);
                if (dir != null)
                {
                    var parameters = (await Database.Parameters.FindAsync(d => d.CriterionId == id));
                    if (parameters.Any())
                    {
                        loggingService.Warn($"Not Active: {dir.CriterionId}");
                        dir.IsActive = false;
                        Database.Criterions.Update(dir);
                    }
                    else
                    {
                        await Database.Criterions.DeleteAsync(id);
                    }
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteCriterion");
                    }
                    else
                    {
                        loggingService.Warn("DeleteCriterion in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteCriterion");
                    }
                }
                loggingService.Warn("DeleteCriterion in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteCriterion");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteCriterion");
            }
        }

        public async Task<OperationDetails> CreateAsync(CriterionDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("CriterionDTO is Empty");
                    return new OperationDetails(false, "Критерий не найден / Criterion is Empty", string.Empty);
                }

                if (model.CriterionId == Guid.Empty || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                var duplicateId = await Database.Criterions.GetAsync(model.CriterionId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in CriterionId = {model.CriterionId}");
                    return new OperationDetails(false, "Критерий с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.Criterions.FindAsync(x => x.Name == model.Name.Trim());
                if (duplName.Any())
                {
                    loggingService.Error($"Duplicate in Name = {model.Name}");
                    return new OperationDetails(false, "Критерий с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                if (model.Order < 1)
                {
                    return new OperationDetails(false, "Номер критерия недействителен / Criterion Code is invalid", "Duplicate");
                }
                // dupl Order
                var duplOrder = await Database.Criterions.FindAsync(x => x.Order == model.Order);
                if (duplOrder.Any())
                {
                    loggingService.Error($"Duplicate in Code = {model.Order}");
                    return new OperationDetails(false, "Критерий с таким номером уже существует / Duplicate by Code", "Duplicate");
                }
                var saveModel = new Criterion
                {
                    CriterionId = model.CriterionId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    Name = model.Name.Trim(),
                    Order = model.Order
                };
                Database.Criterions.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateCriterion");
                }
                else
                {
                    loggingService.Warn("Criterion in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateCriterion");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateCriterion");
            }
        }
    }
}
