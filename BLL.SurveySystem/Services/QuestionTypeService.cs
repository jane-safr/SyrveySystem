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
    public class QuestionTypeService : IQuestionTypeService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<QuestionTypeService> loggingService;
        public QuestionTypeService(IUnitOfWork uow, ILoggerService<QuestionTypeService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<QuestionTypeDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.QuestionTypes.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<QuestionType>, List<QuestionTypeDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<QuestionTypeDTO>();
            }
        }

        public async Task<IEnumerable<QuestionTypeDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.QuestionTypes.GetIQueryable();
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
                            else if (filter.Field.StartsWith("IsFixed", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    inquiry = inquiry.Where(x => x.IsFixedAnswer == val).Include(x=>x.FixedAnswers);
                                }
                            }
                            else if (filter.Field.StartsWith("Searchtxt", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                inquiry = inquiry.Where(x => x.TypeName.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.QuestionTypeId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(50).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<QuestionType>, List<QuestionTypeDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<QuestionTypeDTO>();
            }
        }

        public async Task<QuestionTypeDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new QuestionTypeDTO();
                }
                var dataFirst = await Database.QuestionTypes.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<QuestionType, QuestionTypeDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new QuestionTypeDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(QuestionTypeDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("QuestionTypeDTO is Empty");
                    return new OperationDetails(false, "Тип вопроса не найден / QuestionType is empty", string.Empty);
                }
                if (model.QuestionTypeId == Guid.Empty)
                {
                    loggingService.Error("QuestionTypeId is empty");
                    return new OperationDetails(false, "Id типа отсутствует / QuestionType Id is empty", string.Empty);
                }
                if (string.IsNullOrEmpty(model.TypeName))
                {
                    loggingService.Error("QuestionTypename is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (model.IsFixedAnswer && model.IsOpenAnswer)
                {
                    loggingService.Error("IsFixedAnswer IsOpenAnswer both");
                    return new OperationDetails(false, "Тип не может быть фиксированным и открытым одновременно / IsFixed and IsOpen cannot exist at the same type", string.Empty);
                }

                var questionType = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (questionType == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }
                // duplicate by Name
                var duplName = await Database.QuestionTypes.FindAsync(x => x.TypeName == model.TypeName.Trim() && x.QuestionTypeId != model.QuestionTypeId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Параметр с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
               
                loggingService.Warn($"Update: NameOld:{questionType.TypeName} -> NameNew:{model.TypeName}");
                //Update
                questionType.TypeName = model.TypeName.Trim();
                questionType.IsActive = model.IsActive;
                questionType.IsFixedAnswer = model.IsFixedAnswer;
                questionType.IsOpenAnswer = model.IsOpenAnswer;
                questionType.Comment = string.IsNullOrEmpty(model.Comment) ? null : model.Comment.Trim();

                Database.QuestionTypes.Update(questionType);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateQuestionType");
                }
                else
                {
                    loggingService.Error("UpdateQuestionType error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateQuestionType");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateQuestionType");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty QuestionTypeId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.QuestionTypes.GetAsync(id);
                if (dir != null)
                {
                    var quest = (await Database.Questions.FindAsync(d => d.QuestionTypeId == id));
                    if (quest.Any())
                    {
                        loggingService.Warn($"Not Active: {dir.QuestionTypeId}");
                        dir.IsActive = false;
                        Database.QuestionTypes.Update(dir);
                    }
                    else
                    {
                        await Database.QuestionTypes.DeleteAsync(id);
                    }
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteQuestionType");
                    }
                    else
                    {
                        loggingService.Warn("DeleteQuestionType in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteQuestionType");
                    }
                }
                loggingService.Warn("DeleteQuestionType in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteQuestionType");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteQuestionType");
            }
        }

        public async Task<OperationDetails> CreateAsync(QuestionTypeDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("QuestionTypeDTO is Empty");
                    return new OperationDetails(false, "Параметр не найден / QuestionType is Empty", string.Empty);
                }

                if (model.QuestionTypeId == Guid.Empty || string.IsNullOrEmpty(model.TypeName) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                if (model.IsFixedAnswer && model.IsOpenAnswer)
                {
                    loggingService.Error("IsFixedAnswer IsOpenAnswer both");
                    return new OperationDetails(false, "Тип не может быть фиксированным и открытым одновременно / IsFixed and IsOpen cannot exist at the same type", string.Empty);
                }
                
                var duplicateId = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in QuestionTypeId = {model.QuestionTypeId}");
                    return new OperationDetails(false, "Параметр с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.QuestionTypes.FindAsync(x => x.TypeName == model.TypeName.Trim());
                if (duplName.Any())
                {
                    loggingService.Error($"Duplicate in Name = {model.TypeName}");
                    return new OperationDetails(false, "Тип с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                var saveModel = new QuestionType
                {
                    QuestionTypeId = model.QuestionTypeId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    TypeName = model.TypeName.Trim(),
                    IsFixedAnswer = model.IsFixedAnswer,
                    IsOpenAnswer = model.IsOpenAnswer,
                    Comment = string.IsNullOrEmpty(model.Comment) ? null : model.Comment.Trim()
            };
                Database.QuestionTypes.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateQuestionType");
                }
                else
                {
                    loggingService.Warn("QuestionType in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateQuestionType");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateQuestionType");
            }
        }
    }
}
