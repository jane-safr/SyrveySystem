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
    public class QuestionService : IQuestionService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<QuestionService> loggingService;
        public QuestionService(IUnitOfWork uow, ILoggerService<QuestionService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<QuestionDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.Questions.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Question>, List<QuestionDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<QuestionDTO>();
            }
        }

        public async Task<IEnumerable<QuestionDTO>> GetBySurveyCodeAsync(int code)
        {
            try
            {
                if (code < 1)
                {
                    loggingService.Error("codeSurvey invalid");
                    return new List<QuestionDTO>();
                }

                var survey = (await Database.Surveys.FindAsync(x => x.SurveyCode == code)).FirstOrDefault();
                if (survey == null)
                {
                    loggingService.Error("Survey not found by code");
                    return new List<QuestionDTO>();
                }

                var questions = (await Database.Questions.FindAsync(n => n.SurveyId == survey.SurveyId));
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Question>, List<QuestionDTO>>(questions);
                return results;

            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<QuestionDTO>();
            }
        }

        public async Task<IEnumerable<QuestionDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Questions.GetIQueryable();
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
                                inquiry = inquiry.Where(x => x.QuestionRus.ToLower().Contains(filter.Value.ToLower().Trim()) || x.QuestionEng.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.QuestionId == idGuid);
                                }
                            }
                            else if (filter.Field.StartsWith("SurveyCode", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (int.TryParse(filter.Value, out var code))
                                {
                                    inquiry = inquiry.Where(x => x.Survey.SurveyCode == code);
                                }
                            }
                            else if (filter.Field.StartsWith("Group", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (int.TryParse(filter.Value, out var group))
                                {
                                    inquiry = inquiry.Where(x => x.Group == group);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Question>, List<QuestionDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<QuestionDTO>();
            }
        }

        public async Task<QuestionDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new QuestionDTO();
                }
                var dataFirst = await Database.Questions.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<Question, QuestionDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new QuestionDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(QuestionDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("QuestionDTO is Empty");
                    return new OperationDetails(false, "Показатель не найден / Question is empty", string.Empty);
                }
                if (model.QuestionId == Guid.Empty)
                {
                    loggingService.Error("QuestionId is empty");
                    return new OperationDetails(false, "Id вопроса отсутствует / Question Id is empty", string.Empty);
                }
                if (model.QuestionTypeId == Guid.Empty)
                {
                    loggingService.Error("QuestionTypeId is empty");
                    return new OperationDetails(false, "Выберите тип / Select question type", string.Empty);
                }
                if (model.SurveyId == Guid.Empty)
                {
                    loggingService.Error("SurveyId is empty");
                    return new OperationDetails(false, "Выберите тест / Select survey", string.Empty);
                }
                if (string.IsNullOrEmpty(model.QuestionRus) || string.IsNullOrEmpty(model.QuestionEng))
                {
                    loggingService.Error("Questionname is invalid");
                    return new OperationDetails(false, "Наименование недействительно / Name is invalid", string.Empty);
                }
                if (model.Group < 1)
                {
                    return new OperationDetails(false, "Номер вопроса недействителен / Question Group is invalid", "Duplicate");
                }

                if (model.IsCriterion) // если вопрос критерия - нужен индикатор
                {
                    // проверяем индикатор
                    if (model.IndicatorId == null || model.IndicatorId == Guid.Empty)
                    {
                        return new OperationDetails(false, "Выберите индикатор / Select indicator", string.Empty);
                    }
                    var ind = await Database.Indicators.GetAsync((Guid)model.IndicatorId);
                    if (ind == null)
                    {
                        loggingService.Error("Indicator not found");
                        return new OperationDetails(false, "Индикатор не найден / Indicator not found", string.Empty);
                    }

                    var quest = await Database.Questions.FindAsync(x => x.IndicatorId == model.IndicatorId && x.SurveyId == model.SurveyId && x.QuestionId != model.QuestionId);
                    if(quest.Any())
                    {
                        loggingService.Error("Indicator not found");
                        return new OperationDetails(false, "Вопрос с таким индикатором уже существует / Question with such indicator already exists", string.Empty);
                    }

                }
                else if (model.IndicatorId != null)
                {
                    return new OperationDetails(false, "Отметьте вопрос как вопрос по критерию / Check question as criterion one", string.Empty);
                }

                var qtype = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (qtype == null)
                {
                    loggingService.Error("QuestionType not found");
                    return new OperationDetails(false, "Тип вопроса не найден / Question type not found", string.Empty);
                }

                var surv = await Database.Surveys.GetAsync(model.SurveyId);
                if (surv == null)
                {
                    loggingService.Error("Survey not found");
                    return new OperationDetails(false, "Тест не найден / Survey not found", string.Empty);
                }
                if (!surv.IsActive)
                {
                    loggingService.Error("Survey in not active");
                    return new OperationDetails(false, "Тест не активен / Survey not active", string.Empty);
                }
                if (surv.Invitations.Any(x => x.IsAccepted))
                {
                    loggingService.Error("Test was accepted by some users");
                    return new OperationDetails(false, "Пользователи уже начали прохождение теста. Изменения невозможны / Test was accepted by some users. Cannot update questions", string.Empty);
                }
                // duplicate by Name
                var duplName = await Database.Questions.FindAsync(x => (x.QuestionRus == model.QuestionRus.Trim() && x.QuestionEng == model.QuestionEng.Trim()) && x.SurveyId == model.SurveyId && x.QuestionId != model.QuestionId);
                if (duplName.Any())
                {
                    loggingService.Error("Duplicate by Name");
                    return new OperationDetails(false, "Такой вопрос уже существует / Duplicate by Name", "Duplicate");
                }
                var question = await Database.Questions.GetAsync(model.QuestionId);
                if (question == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }

                // группа вопроса должна быть одинакова у всех фикс
                var fixedAnswerType = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (fixedAnswerType == null)
                {
                    loggingService.Error("QuestionType Fix not found");
                    return new OperationDetails(false, "Тип вопроса не найден / Question Type not found", string.Empty);
                }

                // смотрим, что за есть ли уже такая группа, если нет, то добавляем
                var groupQ = await Database.Questions.FindAsync(x => x.Group == model.Group && x.SurveyId == model.SurveyId && x.QuestionId != model.QuestionId);
                if (groupQ == null || !groupQ.Any())
                {
                    loggingService.Warn("new QuestionGroup added");
                }
                else
                {
                    if (fixedAnswerType.IsFixedAnswer)
                    {
                        if (model.QuestionTypeId != groupQ.FirstOrDefault().QuestionTypeId)
                        {
                            loggingService.Error("QuestionType is not the same for the group");
                            return new OperationDetails(false,
                                $"Тип ответа не соответствует группе {model.Group} / QuestionType does not correspond with Group {model.Group}",
                                string.Empty);
                        }
                    }
                    else
                    {
                        loggingService.Error("there is already such group");
                        return new OperationDetails(false,
                            $"Группа {model.Group} уже существует / Group {model.Group} already exists", string.Empty);
                    }
                }
                
                //если раньше были ответы - удаляем
                if (question.Answers.Any())
                {
                    var answers = (await Database.Answers.FindAsync(x => x.QuestionId == model.QuestionId)).ToList();
                    if (!answers.Any())
                    {
                        loggingService.Warn("no answers to previous fixed answers Question Type");
                    }
                    foreach (var ans in answers)
                    {
                        await Database.Answers.DeleteAsync(ans.AnswerId);
                    }
                }
               
                loggingService.Warn($"Update: NameOld:{question.QuestionId} -> NameNew:{model.QuestionId}");
                //Update
                question.QuestionRus = model.QuestionRus.Trim();
                question.QuestionEng = model.QuestionEng.Trim();
                question.IsActive = model.IsActive;
                question.Group = model.Group;
                question.QuestionTypeId = model.QuestionTypeId;
                question.SurveyId = model.SurveyId;
                question.IndicatorId = model.IndicatorId;
                question.IsInReport = model.IsInReport;
                question.IsCriterion = model.IsCriterion;

                Database.Questions.Update(question);
                var res = await Database.Save();
                if (res > 0)
                {
                    // если заготовленный ответ - пишем в табл Answer
                    if (qtype.IsFixedAnswer)
                    {
                        if (qtype.FixedAnswers.Any())
                        {
                          foreach (var fixAnswer in qtype.FixedAnswers.Where(s => s.IsActive).ToList())
                            {
                                var answer = new Answer
                                {
                                    AnswerId = Guid.NewGuid(),
                                    AnswerRus = fixAnswer.FixAnswerRus.Trim(),
                                    AnswerEng = fixAnswer.FixAnswerEng.Trim(),
                                    IsValid = false,
                                    Credit = fixAnswer.Credit,
                                    QuestionId = model.QuestionId,
                                    CreatedOn = model.CreatedOn,
                                    CreatedBy = model.CreatedBy,
                                    IsActive = true
                                };
                                Database.Answers.Create(answer);
                            }

                            var resansw = await Database.Save();
                            loggingService.Info(resansw > 0
                                ? "Fixed answers were rewritten"
                                : "Fixed answers werent added");
                        }
                        else loggingService.Warn("FixedAnswers not found");
                    }
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateQuestion");
                }
                else
                {
                    loggingService.Error("UpdateQuestion error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateQuestion");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateQuestion");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty QuestionId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var dir = await Database.Questions.GetAsync(id);
                if (dir != null)
                {
                    // при удалении вопроса - уходят все ответы
                    var answers = (await Database.Answers.FindAsync(d => d.QuestionId == id));
                    if (answers != null && answers.Any())
                    {
                        loggingService.Warn($"Deleting ref Answers and Q: {dir.QuestionId}");
                        foreach (var ans in answers)
                        {
                            await Database.Answers.DeleteAsync(ans.AnswerId);
                        }
                    }
                    await Database.Questions.DeleteAsync(id);
                    //dir.IsActive = false;
                    //Database.Questions.Update(dir);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteQuestion");
                    }
                    else
                    {
                        loggingService.Warn("DeleteQuestion in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteQuestion");
                    }
                }

                loggingService.Warn("DeleteQuestion in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteQuestion");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteQuestion");
            }
        }

        public async Task<OperationDetails> CreateAsync(QuestionDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("QuestionDTO is Empty");
                    return new OperationDetails(false, "Вопрос не найден / Question is Empty", string.Empty);
                }

                if (model.QuestionId == Guid.Empty || string.IsNullOrEmpty(model.QuestionRus) || string.IsNullOrEmpty(model.QuestionEng) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }
                if (model.Group < 1)
                {
                    return new OperationDetails(false, "Номер группы недействителен / Question Code is invalid", "Duplicate");
                }
                // проверяем критерий
                if (model.IsCriterion) // если вопрос критерия - то нужен индикатор
                {
                    // проверяем индикатор
                    if (model.IndicatorId == null || model.IndicatorId == Guid.Empty)
                    {
                        return new OperationDetails(false, "Выберите индикатор / Select indicator", string.Empty);
                    }
                    var ind = await Database.Indicators.GetAsync((Guid)model.IndicatorId);
                    if (ind == null)
                    {
                        loggingService.Error("Indicator not found");
                        return new OperationDetails(false, "Индикатор не найден / Indicator not found", string.Empty);
                    }
                    var quest = await Database.Questions.FindAsync(x => x.IndicatorId == model.IndicatorId && x.SurveyId == model.SurveyId);
                    if (quest.Any())
                    {
                        loggingService.Error("Indicator not found");
                        return new OperationDetails(false, "Вопрос с таким индикатором уже существует / Question with such indicator already exists", string.Empty);
                    }
                }
                else if (model.IndicatorId != null)
                {
                    return new OperationDetails(false, "Отметьте вопрос как вопрос по критерию / Check question as criterion one", string.Empty);
                }

                var qtype = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (qtype == null)
                {
                    loggingService.Error("QuestionType not found");
                    return new OperationDetails(false, "Тип вопроса не найден / Question type not found", string.Empty);
                }

                var surv = await Database.Surveys.GetAsync(model.SurveyId);
                if (surv == null)
                {
                    loggingService.Error("Survey not found");
                    return new OperationDetails(false, "Тест не найден / Survey not found", string.Empty);
                }
                if (!surv.IsActive)
                {
                    loggingService.Error("Survey in not active");
                    return new OperationDetails(false, "Тест не активен / Survey not active", string.Empty);
                }
                if (surv.Invitations.Any(x => x.IsAccepted))
                {
                    loggingService.Error("Test was accepted by some users");
                    return new OperationDetails(false, "Пользователи уже начали прохождение теста. Изменения невозможны / Test was accepted by some users. Cannot update questions", string.Empty);
                }

                var duplicateId = await Database.Questions.GetAsync(model.QuestionId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in QuestionId = {model.QuestionId}");
                    return new OperationDetails(false, "Параметр с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // dupl Name
                var duplName = await Database.Questions.FindAsync(x => x.QuestionRus == model.QuestionRus.Trim() && x.QuestionEng == model.QuestionEng.Trim() && x.SurveyId == model.SurveyId);
                if (duplName.Any())
                {
                    loggingService.Error($"Duplicate in Name = {model.QuestionId}");
                    return new OperationDetails(false, "Вопрос с таким именем уже существует / Duplicate by Name", "Duplicate");
                }
                // группа вопроса должна быть одинакова у всех фикс
                var fixedAnswerType = await Database.QuestionTypes.GetAsync(model.QuestionTypeId);
                if (fixedAnswerType == null)
                {
                    loggingService.Error("QuestionType Fix not found");
                    return new OperationDetails(false, "Тип вопроса не найден / Question Type not found", string.Empty);
                }

                // смотрим, что за есть ли уже такая группа, если нет, то добавляем
                var groupQ = await Database.Questions.FindAsync(x => x.Group == model.Group && x.SurveyId == model.SurveyId);
                if (groupQ == null || !groupQ.Any())
                {
                    loggingService.Warn("new QuestionGroup added");
                }
                else
                {
                    if (fixedAnswerType.IsFixedAnswer)
                    {
                        if (model.QuestionTypeId != groupQ.FirstOrDefault().QuestionTypeId)
                        {
                            loggingService.Error("QuestionType is not the same for the group");
                            return new OperationDetails(false,
                                $"Тип ответа не соответствует группе {model.Group} / QuestionType does not correspond with Group {model.Group}",
                                string.Empty);
                        }
                    }
                    else
                    {
                        loggingService.Error("there is already such group");
                        return new OperationDetails(false,
                            $"Группа {model.Group} уже существует / Group {model.Group} already exists", string.Empty);
                    }
                }

                var saveModel = new Question
                {
                    QuestionId = model.QuestionId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    SurveyId = model.SurveyId,
                    IndicatorId = model.IndicatorId,
                    QuestionTypeId = model.QuestionTypeId,
                    IsInReport = model.IsInReport,
                    IsCriterion = model.IsCriterion,
                    QuestionRus = model.QuestionRus.Trim(),
                    QuestionEng = model.QuestionEng.Trim(),
                    Group = model.Group
                };
                
                Database.Questions.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    // если заготовленный ответ - пишем в табл Answer
                    if (qtype.IsFixedAnswer)
                    {
                        if (qtype.FixedAnswers.Any())
                        {
                            foreach (var fixAnswer in qtype.FixedAnswers.Where(s => s.IsActive).ToList())
                            {
                                var answer = new Answer
                                {
                                    AnswerId =Guid.NewGuid(),
                                    AnswerRus = fixAnswer.FixAnswerRus.Trim(),
                                    AnswerEng = fixAnswer.FixAnswerEng.Trim(),
                                    IsValid = false,
                                    Credit = fixAnswer.Credit,
                                    QuestionId = model.QuestionId,
                                    CreatedOn = model.CreatedOn,
                                    CreatedBy = model.CreatedBy,
                                    IsActive = true
                                };
                                Database.Answers.Create(answer);
                            }

                            await Database.Save();
                        }
                        else loggingService.Warn("FixedAnswers not found");
                    }
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateQuestion");
                }
                else
                {
                    loggingService.Warn("Question in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateQuestion");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateQuestion");
            }
        }
    }
}
