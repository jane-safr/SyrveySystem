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
    public class FixedAnswerService : IFixedAnswerService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<FixedAnswerService> loggingService;
        public FixedAnswerService(IUnitOfWork uow, ILoggerService<FixedAnswerService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<FixedAnswerDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.FixedAnswers.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<FixedAnswer>, List<FixedAnswerDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<FixedAnswerDTO>();
            }
        }

        public async Task<IEnumerable<FixedAnswerDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.FixedAnswers.GetIQueryable();
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
                                inquiry = inquiry.Where(x => x.FixAnswerRus.ToLower().Contains(filter.Value.ToLower().Trim()) || x.FixAnswerEng.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.FixedAnswerId == idGuid);
                                }
                            }
                            else if (filter.Field.StartsWith("QuestionTypeId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.QuestionTypeId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<FixedAnswer>, List<FixedAnswerDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<FixedAnswerDTO>();
            }
        }

        public async Task<FixedAnswerDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new FixedAnswerDTO();
                }
                var dataFirst = await Database.FixedAnswers.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<FixedAnswer, FixedAnswerDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new FixedAnswerDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(FixedAnswerDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("FixedAnswerDTO is Empty");
                    return new OperationDetails(false, "Фиксированный ответ не найден / Fixed Answer is empty", string.Empty);
                }
                if (model.FixedAnswerId == Guid.Empty)
                {
                    loggingService.Error("FixedAnswerId is empty");
                    return new OperationDetails(false, "Id ответа отсутствует / FixedAnswerId is empty", string.Empty);
                }
                if (model.QuestionTypeId == Guid.Empty)
                {
                    loggingService.Error("QuestionTypeId is empty");
                    return new OperationDetails(false, "Id типа вопроса отсутствует / QuestionTypeId is empty", string.Empty);
                }
                if (string.IsNullOrEmpty(model.FixAnswerEng) || string.IsNullOrEmpty(model.FixAnswerRus))
                {
                    loggingService.Error("FixedAnswer is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (model.Credit < 0)
                {
                    loggingService.Error($"Credit invalid {model.Credit}");
                    return new OperationDetails(false, "Балл недействителен / Credit is invalid", "Duplicate");
                }
                var fixedAnswer = await Database.FixedAnswers.GetAsync(model.FixedAnswerId);
                if (fixedAnswer == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }

                // dupl Name
                var duplName = await Database.FixedAnswers.FindAsync(x => (x.FixAnswerRus == model.FixAnswerRus.Trim() || x.FixAnswerEng == model.FixAnswerEng.Trim()) && x.QuestionTypeId == model.QuestionTypeId && x.FixedAnswerId != model.FixedAnswerId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Фиксированный ответ с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                var qType = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (qType == null)
                {
                    loggingService.Error("Question Type not found");
                    return new OperationDetails(false, "Тип вопроса не найден / Question Type not found", "Duplicate");
                }
                if (!qType.IsFixedAnswer)
                {
                    loggingService.Error("Question Type not fixed");
                    return new OperationDetails(false, "Тип вопроса не является фикс. / Question Type not fixed", "Duplicate");
                }
                
                loggingService.Warn($"Update: NameOld:{fixedAnswer.FixAnswerEng} -> NameNew:{model.FixAnswerEng}");
                //Update
                fixedAnswer.FixAnswerRus = model.FixAnswerRus.Trim();
                fixedAnswer.FixAnswerEng = model.FixAnswerEng.Trim();
                fixedAnswer.Credit = model.Credit;
                fixedAnswer.QuestionTypeId = model.QuestionTypeId;
                fixedAnswer.IsActive = model.IsActive;
                
                Database.FixedAnswers.Update(fixedAnswer);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateFixedAnswer");
                }
                else
                {
                    loggingService.Error("UpdateFixedAnswer error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateFixedAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateFixedAnswer");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty FixedAnswerId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.FixedAnswers.GetAsync(id);
                if (dir != null)
                {
                    await Database.FixedAnswers.DeleteAsync(id);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteFixedAnswer");
                    }
                    else
                    {
                        loggingService.Warn("DeleteFixedAnswer in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteFixedAnswer");
                    }
                }
                loggingService.Warn("DeleteFixedAnswer in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteFixedAnswer");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteFixedAnswer");
            }
        }

        public async Task<OperationDetails> CreateAsync(FixedAnswerDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("FixedAnswerDTO is Empty");
                    return new OperationDetails(false, "Фиксированный ответ не найден / Fixed Answer is Empty", string.Empty);
                }

                if (model.FixedAnswerId == Guid.Empty || model.QuestionTypeId == Guid.Empty || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }

                if(string.IsNullOrEmpty(model.FixAnswerRus) || string.IsNullOrEmpty(model.FixAnswerEng))
                {
                    loggingService.Error("Name is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                var duplicateId = await Database.FixedAnswers.GetAsync(model.FixedAnswerId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in FixedAnswerId = {model.FixedAnswerId}");
                    return new OperationDetails(false, "Ответ с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.FixedAnswers.FindAsync(x => (x.FixAnswerRus == model.FixAnswerRus.Trim() || x.FixAnswerEng == model.FixAnswerEng.Trim()) && x.QuestionTypeId==model.QuestionTypeId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Фиксированный ответ с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                if (model.Credit < 0)
                {
                    loggingService.Error($"Credit invalid {model.Credit}");
                    return new OperationDetails(false, "Балл недействителен / Credit is invalid", "Duplicate");
                }
                var qType = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (qType == null)
                {
                    loggingService.Error("Question Type not found");
                    return new OperationDetails(false, "Тип вопроса не найден / Question Type not found", "Duplicate");
                }
                if (!qType.IsFixedAnswer)
                {
                    loggingService.Error("Question Type not fixed");
                    return new OperationDetails(false, "Тип вопроса не является фикс. / Question Type not fixed", "Duplicate");
                }
                
                var saveModel = new FixedAnswer
                {
                    FixedAnswerId = model.FixedAnswerId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    FixAnswerRus = model.FixAnswerRus.Trim(),
                    FixAnswerEng = model.FixAnswerEng.Trim(),
                    Credit = model.Credit,
                    QuestionTypeId = model.QuestionTypeId
                };
                Database.FixedAnswers.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateFixedAnswer");
                }
                else
                {
                    loggingService.Warn("FixedAnswer in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateFixedAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateFixedAnswer");
            }
        }
    }
}
