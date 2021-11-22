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
   public class ParameterService : IParameterService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<ParameterService> loggingService;
        public ParameterService(IUnitOfWork uow, ILoggerService<ParameterService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<ParameterDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.Parameters.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Parameter>, List<ParameterDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<ParameterDTO>();
            }
        }

        public async Task<IEnumerable<ParameterDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Parameters.GetIQueryable();
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
                                    inquiry = inquiry.Where(x => x.ParameterId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Parameter>, List<ParameterDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<ParameterDTO>();
            }
        }

        public async Task<ParameterDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new ParameterDTO();
                }
                var dataFirst = await Database.Parameters.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<Parameter, ParameterDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new ParameterDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(ParameterDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("ParameterDTO is Empty");
                    return new OperationDetails(false, "Показатель не найден / Parameter is empty", string.Empty);
                }
                if (model.ParameterId == Guid.Empty)
                {
                    loggingService.Error("ParameterId is empty");
                    return new OperationDetails(false, "Id параметра отсутствует / Parameter Id is empty", string.Empty);
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    loggingService.Error("Parametername is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (model.Order < 1)
                {
                    return new OperationDetails(false, "Номер параметра недействителен / Parameter Code is invalid", "Duplicate");
                }
                // проверяем критерий
                if (model.CriterionId == Guid.Empty)
                {
                    return new OperationDetails(false, "Выберите критерий / Select criterion", string.Empty);
                }
                var criterion = await Database.Criterions.GetAsync(model.CriterionId);
                if (criterion == null)
                {
                    return new OperationDetails(false, "Критерий не найден / Criterion not found", string.Empty);
                }

                var parameter = await Database.Parameters.GetAsync(model.ParameterId);
                if (parameter == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }
                // duplicate by Name
                var duplName = await Database.Parameters.FindAsync(x => x.Name == model.Name.Trim() && x.ParameterId != model.ParameterId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Параметр с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                // dupl Order
                var duplOrder = await Database.Parameters.FindAsync(x => x.Order == model.Order && x.ParameterId != model.ParameterId && x.CriterionId == model.CriterionId);
                if (duplOrder.Any())
                {
                    loggingService.Error($"Duplicate in Code = {model.Order}");
                    return new OperationDetails(false, "Параметр с таким номером уже существует / Duplicate by Code", "Duplicate");
                }
                loggingService.Warn($"Update: NameOld:{parameter.Name} -> NameNew:{model.Name}");
                //Update
                parameter.Name = model.Name.Trim();
                parameter.IsActive = model.IsActive;
                parameter.Order = model.Order;
                parameter.CriterionId = model.CriterionId;

                Database.Parameters.Update(parameter);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateParameter");
                }
                else
                {
                    loggingService.Error("UpdateParameter error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateParameter");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateParameter");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty ParameterId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.Parameters.GetAsync(id);
                if (dir != null)
                {
                    var parameters = (await Database.Indicators.FindAsync(d => d.ParameterId == id));
                    if (parameters.Any())
                    {
                        loggingService.Warn($"Not Active: {dir.ParameterId}");
                        dir.IsActive = false;
                        Database.Parameters.Update(dir);
                    }
                    else
                    {
                        await Database.Parameters.DeleteAsync(id);
                    }
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteParameter");
                    }
                    else
                    {
                        loggingService.Warn("DeleteParameter in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteParameter");
                    }
                }
                loggingService.Warn("DeleteParameter in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteParameter");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteParameter");
            }
        }

        public async Task<OperationDetails> CreateAsync(ParameterDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("ParameterDTO is Empty");
                    return new OperationDetails(false, "Параметр не найден / Parameter is Empty", string.Empty);
                }

                if (model.ParameterId == Guid.Empty || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                if (model.Order < 1)
                {
                    return new OperationDetails(false, "Номер параметра недействителен / Parameter Code is invalid", "Duplicate");
                }
                // проверяем критерий
                if (model.CriterionId == Guid.Empty)
                {
                    return new OperationDetails(false, "Выберите критерий / Select criterion", string.Empty);
                }
                var criterion = await Database.Criterions.GetAsync(model.CriterionId);
                if (criterion == null)
                {
                    return new OperationDetails(false, "Критерий не найден / Criterion not found", string.Empty);
                }
                var duplicateId = await Database.Parameters.GetAsync(model.ParameterId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in ParameterId = {model.ParameterId}");
                    return new OperationDetails(false, "Параметр с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.Parameters.FindAsync(x => x.Name == model.Name.Trim());
                if (duplName.Any())
                {
                    loggingService.Error($"Duplicate in Name = {model.Name}");
                    return new OperationDetails(false, "Параметр с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                // dupl Order
                var duplOrder = await Database.Parameters.FindAsync(x => x.Order == model.Order && x.CriterionId == model.CriterionId);
                if (duplOrder.Any())
                {
                    loggingService.Error($"Duplicate in Code = {model.Order}");
                    return new OperationDetails(false, "Параметр с таким номером уже существует / Duplicate by Code", "Duplicate");
                }
                var saveModel = new Parameter
                {
                    ParameterId = model.ParameterId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    CriterionId = model.CriterionId,
                    Name = model.Name.Trim(),
                    Order = model.Order
                };
                Database.Parameters.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateParameter");
                }
                else
                {
                    loggingService.Warn("Parameter in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateParameter");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateParameter");
            }
        }
    }
}
