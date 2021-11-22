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
    public class SurveyTypeService : ISurveyTypeService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<SurveyTypeService> loggingService;
        public SurveyTypeService(IUnitOfWork uow, ILoggerService<SurveyTypeService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<SurveyTypeDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.SurveyTypes.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<SurveyType>, List<SurveyTypeDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<SurveyTypeDTO>();
            }
        }

        public async Task<IEnumerable<SurveyTypeDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.SurveyTypes.GetIQueryable();
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
                                inquiry = inquiry.Where(x => x.NameRus.ToLower().Contains(filter.Value.ToLower().Trim())
                                                             || x.NameEng.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.SurveyTypeId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<SurveyType>, List<SurveyTypeDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<SurveyTypeDTO>();
            }
        }

        public async Task<SurveyTypeDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new SurveyTypeDTO();
                }
                var dataFirst = await Database.SurveyTypes.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<SurveyType, SurveyTypeDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new SurveyTypeDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(SurveyTypeDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("SurveyTypeDTO is Empty");
                    return new OperationDetails(false, "Тип не найден / Survey Type is empty", string.Empty);
                }
                if (model.SurveyTypeId == Guid.Empty)
                {
                    loggingService.Error("SurveyTypeId is empty");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (string.IsNullOrEmpty(model.NameRus) || string.IsNullOrEmpty(model.NameEng))
                {
                    loggingService.Error("SurveyTypeRus-Eng is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (string.IsNullOrEmpty(model.DescriptionRus) || string.IsNullOrEmpty(model.DescriptionEng))
                {
                    loggingService.Error("DescriptionRus-Eng is invalid");
                    return new OperationDetails(false, "Описание недействительно / Description is invalid", string.Empty);
                }
                var type = await Database.SurveyTypes.GetAsync(model.SurveyTypeId);
                if (type == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }
               // duplicate by Name
                var duplName = await Database.SurveyTypes.FindAsync(x => (x.NameRus == model.NameRus.Trim() || x.NameEng == model.NameEng.Trim()) && x.SurveyTypeId != model.SurveyTypeId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Тип с таким именем уже существует / Duplicate by Name", "Duplicate");
                }

                loggingService.Warn($"Update: NameOld:{type.NameRus} -> NameNew:{model.NameRus}");
                loggingService.Warn($"Update: NameOld:{type.NameEng} -> NameNew:{model.NameEng}");
                //Update
                type.NameRus = model.NameRus.Trim();
                type.NameEng = model.NameEng.Trim();
                type.IsActive = model.IsActive;
                type.DescriptionRus = model.DescriptionRus.Trim();
                type.DescriptionEng = model.DescriptionEng.Trim();

                Database.SurveyTypes.Update(type);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateSurveyType");
                }
                else
                {
                    loggingService.Error("UpdateSurveyType error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateSurveyType");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateSurveyType");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty SurveyTypeId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.SurveyTypes.GetAsync(id);
                if (dir != null)
                {
                    var surveys = (await Database.Surveys.FindAsync(d => d.SurveyTypeId == id));
                    if (surveys.Any())
                    {
                        loggingService.Warn($"Not Active: {dir.SurveyTypeId}");
                        dir.IsActive = false;
                        Database.SurveyTypes.Update(dir);
                    }
                    else
                    {
                        await Database.SurveyTypes.DeleteAsync(id);
                    }
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteSurveyType");
                    }
                    else
                    {
                        loggingService.Warn("DeleteSurveyType in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteSurveyType");
                    }
                }
                loggingService.Warn("DeleteSurveyType in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteSurveyType");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteSurveyType");
            }
        }

        public async Task<OperationDetails> CreateAsync(SurveyTypeDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("SurveyTypeDTO is Empty");
                    return new OperationDetails(false, "Тип не найден / Survey Type is Empty", string.Empty);
                }

                if (model.SurveyTypeId == Guid.Empty || string.IsNullOrEmpty(model.NameRus) || string.IsNullOrEmpty(model.NameEng) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (string.IsNullOrEmpty(model.DescriptionRus) || string.IsNullOrEmpty(model.DescriptionEng))
                {
                    loggingService.Error("DescriptionRus-Eng is invalid");
                    return new OperationDetails(false, "Описание недействительно / Description is invalid", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                var duplicateId = await Database.SurveyTypes.GetAsync(model.SurveyTypeId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in SurveyTypeId = {model.SurveyTypeId}");
                    return new OperationDetails(false, "Тип с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.SurveyTypes.FindAsync(x => x.NameRus == model.NameRus.Trim() ||  x.NameEng == model.NameEng.Trim());
                if (duplName.Any())
                {
                    loggingService.Error($"Duplicate in Name = {model.NameRus} || {model.NameEng}");
                    return new OperationDetails(false, "Тип с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                var saveModel = new SurveyType
                {
                    SurveyTypeId = model.SurveyTypeId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    NameRus = model.NameRus.Trim(),
                    NameEng = model.NameEng.Trim(),
                    DescriptionRus = model.DescriptionRus.Trim(),
                    DescriptionEng = model.DescriptionEng.Trim()
                };
                Database.SurveyTypes.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateSurveyType");
                }
                else
                {
                    loggingService.Warn("CreateSurveyType in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateSurveyType");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateSurveyType");
            }
        }
    }
}
