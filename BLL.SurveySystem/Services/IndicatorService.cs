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
    public class IndicatorService : IIndicatorService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<IndicatorService> loggingService;
        public IndicatorService(IUnitOfWork uow, ILoggerService<IndicatorService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<IndicatorDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.Indicators.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Indicator>, List<IndicatorDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<IndicatorDTO>();
            }
        }

        public async Task<IEnumerable<IndicatorDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Indicators.GetIQueryable();
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
                                    inquiry = inquiry.Where(x => x.IndicatorId == idGuid);
                                }
                            }
                            else if (filter.Field.StartsWith("ParameterId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
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
                var results = mapper.Map<IEnumerable<Indicator>, List<IndicatorDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<IndicatorDTO>();
            }
        }

        public async Task<IndicatorDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new IndicatorDTO();
                }
                var dataFirst = await Database.Indicators.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<Indicator, IndicatorDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new IndicatorDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(IndicatorDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("IndicatorDTO is Empty");
                    return new OperationDetails(false, "Индикатор не найден / Indicator is empty", string.Empty);
                }
                if (model.IndicatorId == Guid.Empty)
                {
                    loggingService.Error("IndicatorId is empty");
                    return new OperationDetails(false, "Id индикатора отсутствует / Indicator Id is empty", string.Empty);
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    loggingService.Error("Indicatorname is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (model.Order < 1)
                {
                    return new OperationDetails(false, "Номер индикатора недействителен / Indicator Code is invalid", "Duplicate");
                }
                // проверяем показатель
                if (model.ParameterId == Guid.Empty)
                {
                    return new OperationDetails(false, "Выберите показатель / Select parameter", string.Empty);
                }
                var param = await Database.Parameters.GetAsync(model.ParameterId);
                if (param == null)
                {
                    return new OperationDetails(false, "Показатель не найден / Parameter not found", string.Empty);
                }

                var indicator = await Database.Indicators.GetAsync(model.IndicatorId);
                if (indicator == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }
                // duplicate by Name
                var duplName = await Database.Indicators.FindAsync(x => x.Name == model.Name.Trim() && x.ParameterId == model.ParameterId && x.IndicatorId != model.IndicatorId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Индикатор с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                // dupl Order
                var duplOrder = await Database.Indicators.FindAsync(x => x.Order == model.Order && x.IndicatorId != model.IndicatorId && x.ParameterId == model.ParameterId && x.IsActive);
                if (duplOrder.Any())
                {
                    loggingService.Error($"Duplicate in Code = {model.Order}");
                    return new OperationDetails(false, "Индикатор с таким номером уже существует / Duplicate by Code", "Duplicate");
                }
                loggingService.Warn($"Update: NameOld:{indicator.Name} -> NameNew:{model.Name}");
                //Update
                indicator.Name = model.Name.Trim();
                indicator.IsActive = model.IsActive;
                indicator.Order = model.Order;
                indicator.ParameterId = model.ParameterId;

                Database.Indicators.Update(indicator);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateIndicator");
                }
                else
                {
                    loggingService.Error("UpdateIndicator error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateIndicator");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateIndicator");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty IndicatorId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var indicator = await Database.Indicators.GetAsync(id);
                if (indicator != null)
                {
                    // Questions - если есть привязанные вопросы
                    var questions = await Database.Questions.FindAsync(x => x.IndicatorId == id);
                    if (questions.Any())
                    {
                        loggingService.Warn($"Not Active: {indicator.IndicatorId}");
                        indicator.IsActive = false;
                        Database.Indicators.Update(indicator);
                    }
                    else
                    {
                        await Database.Indicators.DeleteAsync(id);
                    }
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteIndicator");
                    }
                    else
                    {
                        loggingService.Warn("DeleteIndicator in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteIndicator");
                    }
                }
                loggingService.Warn("DeleteIndicator in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteIndicator");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteIndicator");
            }
        }

        public async Task<OperationDetails> CreateAsync(IndicatorDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("IndicatorDTO is Empty");
                    return new OperationDetails(false, "Индикатор не найден / Indicator is Empty", string.Empty);
                }

                if (model.IndicatorId == Guid.Empty || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.CreatedBy))
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
                    return new OperationDetails(false, "Номер индикатора недействителен / Indicator Code is invalid", "Duplicate");
                }
                // проверяем показатель
                if (model.ParameterId == Guid.Empty)
                {
                    return new OperationDetails(false, "Выберите показатель / Select parameter", string.Empty);
                }
                var par = await Database.Parameters.GetAsync(model.ParameterId);
                if (par == null)
                {
                    return new OperationDetails(false, "Показатель не найден / Parameter not found", string.Empty);
                }
                var duplicateId = await Database.Indicators.GetAsync(model.IndicatorId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in IndicatorId = {model.IndicatorId}");
                    return new OperationDetails(false, "Индикатор с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.Indicators.FindAsync(x => x.Name == model.Name.Trim() && x.ParameterId == model.ParameterId);
                if (duplName.Any())
                {
                    loggingService.Error($"Duplicate in Name = {model.Name}");
                    return new OperationDetails(false, "Индикатор с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                // dupl Order
                var duplOrder = await Database.Indicators.FindAsync(x => x.Order == model.Order && x.ParameterId == model.ParameterId && x.IsActive);
                if (duplOrder.Any())
                {
                    loggingService.Error($"Duplicate in Code = {model.Order}");
                    return new OperationDetails(false, "Индикатор с таким номером уже существует / Duplicate by Code", "Duplicate");
                }
                var saveModel = new Indicator
                {
                    IndicatorId = model.IndicatorId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    ParameterId = model.ParameterId,
                    Name = model.Name.Trim(),
                    Order = model.Order
                };
                Database.Indicators.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateIndicator");
                }
                else
                {
                    loggingService.Warn("Indicator in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateIndicator");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateIndicator");
            }
        }
    }
}
