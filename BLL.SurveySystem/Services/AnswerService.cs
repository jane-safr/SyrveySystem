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
    public class AnswerService : IAnswerService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<AnswerService> loggingService;
        public AnswerService(IUnitOfWork uow, ILoggerService<AnswerService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<AnswerDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.Answers.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Answer>, List<AnswerDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<AnswerDTO>();
            }
        }

        public async Task<IEnumerable<AnswerDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Answers.GetIQueryable();
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
                                inquiry = inquiry.Where(x => x.AnswerRus.ToLower().Contains(filter.Value.ToLower().Trim()) || x.AnswerEng.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.AnswerId == idGuid);
                                }
                            }
                            else if (filter.Field.StartsWith("QuestionId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.QuestionId == idGuid);
                                }
                            }
                            else if (filter.Field.StartsWith("SurveyId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.Question.SurveyId == idGuid);
                                }
                            }
                            else if (filter.Field.StartsWith("QuestionTypeId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.Question.QuestionTypeId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Answer>, List<AnswerDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<AnswerDTO>();
            }
        }

        public async Task<AnswerDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new AnswerDTO();
                }
                var dataFirst = await Database.Answers.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<Answer, AnswerDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new AnswerDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(AnswerDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("AnswerDTO is Empty");
                    return new OperationDetails(false, "Ответ не найден / Answer is empty", string.Empty);
                }
                if (model.AnswerId == Guid.Empty)
                {
                    loggingService.Error("AnswerId is empty");
                    return new OperationDetails(false, "Id ответа отсутствует / AnswerId is empty", string.Empty);
                }
                if (string.IsNullOrEmpty(model.AnswerRus) || string.IsNullOrEmpty(model.AnswerEng))
                {
                    loggingService.Error("Answer is not filled in");
                    return new OperationDetails(false, "Ответ не заполнен / Answer is not filled in", string.Empty);
                }
                if (model.QuestionId == Guid.Empty)
                {
                    loggingService.Error("Question is invalid");
                    return new OperationDetails(false, "Вопрос не выбран / Question is invalid", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                if (model.Credit < 0)
                {
                    return new OperationDetails(false, "Балл недействителен / Credit is invalid", string.Empty);
                }
                // dupl Name ТОЛЬКО РУС
                var duplName = await Database.Answers.FindAsync(x => x.AnswerRus == model.AnswerRus.Trim() && x.AnswerId!=model.AnswerId && x.QuestionId==model.QuestionId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate in Name");
                    return new OperationDetails(false, "Такой ответ уже существует / Duplicate by Name", "Duplicate");
                }
                // проверяем вопрос
                var question = await Database.Questions.GetAsync(model.QuestionId);
                if (question == null)
                {
                    loggingService.Error("Question not found");
                    return new OperationDetails(false, "Вопрос не найден / Question not found", string.Empty);
                }
                var answer = await Database.Answers.GetAsync(model.AnswerId);
                if (answer == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }
                
               loggingService.Warn($"Update: NameOld:{answer.AnswerEng} -> NameNew:{model.AnswerEng}");
                //Update
                answer.AnswerRus = model.AnswerRus.Trim();
                answer.AnswerEng = model.AnswerEng.Trim();
                answer.QuestionId = model.QuestionId;
                answer.IsActive = model.IsActive;
                answer.IsValid = model.IsValid;
                answer.Credit = model.Credit;

                Database.Answers.Update(answer);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateAnswer");
                }
                else
                {
                    loggingService.Error("UpdateAnswer error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateAnswer");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty AnswerId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.Answers.GetAsync(id);
                if (dir != null)
                {
                   // TODO проверка
                    await Database.Answers.DeleteAsync(id);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteAnswer");
                    }
                    else
                    {
                        loggingService.Warn("DeleteAnswer in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteAnswer");
                    }
                }
                loggingService.Warn("DeleteAnswer in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteAnswer");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteAnswer");
            }
        }

        public async Task<OperationDetails> CreateAsync(AnswerDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("AnswerDTO is Empty");
                    return new OperationDetails(false, "Критерий не найден / Answer is Empty", string.Empty);
                }
                if (model.AnswerId == Guid.Empty || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.QuestionId == Guid.Empty)
                {
                    loggingService.Error("Question is invalid");
                    return new OperationDetails(false, "Вопрос не выбран / Question is invalid", string.Empty);
                }
                if (string.IsNullOrEmpty(model.AnswerRus) || string.IsNullOrEmpty(model.AnswerEng))
                {
                    loggingService.Error("Answer is not filled in");
                    return new OperationDetails(false, "Ответ не заполнен / Answer is not filled in", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                var duplicateId = await Database.Answers.GetAsync(model.AnswerId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in AnswerId = {model.AnswerId}");
                    return new OperationDetails(false, "Критерий с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name ТОЛЬКО РУС
                var duplName = await Database.Answers.FindAsync(x => x.AnswerRus == model.AnswerRus.Trim() && x.QuestionId == model.QuestionId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate in Name");
                    return new OperationDetails(false, "Такой ответ уже существует / Duplicate by Name", "Duplicate");
                }
                if (model.Credit < 0)
                {
                    return new OperationDetails(false, "Балл недействителен / Credit is invalid", string.Empty);
                }
                // проверяем вопрос
                var question = await Database.Questions.GetAsync(model.QuestionId);
                if (question == null)
                {
                    loggingService.Error("question not found");
                    return new OperationDetails(false, "Вопрос не найден / Question not found", string.Empty);
                }
               
                var saveModel = new Answer
                {
                    AnswerId = model.AnswerId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    IsValid = model.IsValid,
                    AnswerRus = model.AnswerRus.Trim(),
                    AnswerEng = model.AnswerEng.Trim(),
                    QuestionId = model.QuestionId,
                    Credit = model.Credit
                };
                Database.Answers.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateAnswer");
                }
                else
                {
                    loggingService.Warn("Answer in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateAnswer");
            }
        }
    }
}
