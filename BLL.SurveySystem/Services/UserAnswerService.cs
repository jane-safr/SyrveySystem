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
    public class UserAnswerService : IUserAnswerService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<UserAnswerService> loggingService;

        public UserAnswerService(IUnitOfWork uow, ILoggerService<UserAnswerService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }

        public async Task<IEnumerable<UserAnswerDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.UserAnswers.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<UserAnswer>, List<UserAnswerDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<UserAnswerDTO>();
            }
        }

        public async Task<IEnumerable<UserAnswerDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.UserAnswers.GetIQueryable();
                if (filterModels != null && filterModels.Any())
                {
                    foreach (var filter in filterModels)
                    {
                        if (filter != null)
                        {
                            if (filter.Field.StartsWith("IsActive", StringComparison.OrdinalIgnoreCase) &&
                                !string.IsNullOrEmpty(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    inquiry = inquiry.Where(x => x.IsActive == val);
                                }
                            }
                            else if (filter.Field.StartsWith("Searchtxt", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrEmpty(filter.Value))
                            {
                                inquiry = inquiry.Where(x =>
                                    x.Invitation.UserName.ToLower().Contains(filter.Value.ToLower().Trim()) ||
                                    x.Invitation.UserEmail.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.UserAnswerId == idGuid);
                                }
                            }
                            //InvitationId
                            else if (filter.Field.StartsWith("InvitationId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var id))
                                {
                                    inquiry = inquiry.Where(x => x.InvitationId == id);
                                }
                            }
                            else if (filter.Field.StartsWith("AnswerId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var id))
                                {
                                    inquiry = id == Guid.Empty ? inquiry.Where(x => x.AnswerId == null) : inquiry.Where(x => x.AnswerId == id);
                                }
                            }
                        }
                    }
                }

                var fullData = await inquiry.AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<UserAnswer>, List<UserAnswerDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<UserAnswerDTO>();
            }
        }

        public async Task<UserAnswerDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new UserAnswerDTO();
                }

                var dataFirst = await Database.UserAnswers.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<UserAnswer, UserAnswerDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new UserAnswerDTO();
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty UserAnswerId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }

                var inv = await Database.UserAnswers.GetAsync(id);
                if (inv != null)
                {
                    await Database.UserAnswers.DeleteAsync(id);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteUserAnswer");
                    }
                    else
                    {
                        loggingService.Warn("DeleteUserAnswer in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteUserAnswer");
                    }
                }

                loggingService.Warn("DeleteUserAnswer in database None");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteUserAnswer");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteUserAnswer");
            }
        }

        public async Task<OperationDetails> CreateByInvitationAsync(Guid invitationId)
        {
            try
            {
                if (invitationId == Guid.Empty)
                {
                    loggingService.Error("invitationId is Empty");
                    return new OperationDetails(false, "Приглашение не найдено / InvitationId is Empty", string.Empty);
                }
                var invitation = await Database.Invitations.GetAsync(invitationId);
                if (invitation == null)
                {
                    loggingService.Error("invitation is Empty");
                    return new OperationDetails(false, "Приглашение не найдено / Invitation is Empty", string.Empty);
                }

                var userAnswers = await Database.UserAnswers.FindAsync(x => x.InvitationId == invitationId);
                if (userAnswers.Any())
                {
                    loggingService.Error("userAnswers were already created");
                    return new OperationDetails(false, "Тест уже был инициирован / Survey has already been initiated", string.Empty);
                }

                var questionsMain = (await Database.Questions.FindAsync(x => x.SurveyId == invitation.SurveyId)).ToList();
                if (!questionsMain.Any())
                {
                    loggingService.Error("Questions not found");
                    return new OperationDetails(false, "Вопросы не найдены / Questions not found", string.Empty);
                }
                var index = 1;
                foreach (var question in questionsMain)
                {
                    if (question.QuestionId == Guid.Empty)
                    {
                        loggingService.Error("QuestionId is empty");
                        continue;
                    }
                    // чтобы не дублировать вопросы
                    var userAnswExist = await Database.UserAnswers.FindAsync(x => x.QuestionId == question.QuestionId && x.InvitationId == invitationId);
                    if (userAnswExist.Any())
                    {
                        loggingService.Error($"Question {question.QuestionId} for this invit already exists: skip");
                        continue;
                    }
                    if (!question.Answers.Any() && !question.QuestionType.IsOpenAnswer)
                    {
                        loggingService.Error($"Question {question.QuestionId} does not have answers: skip");
                        continue;
                    }
                    var saveModelAnsw = new UserAnswer
                    {
                        UserAnswerId = Guid.NewGuid(),
                        CreatedOn = DateTime.Now,
                        CreatedBy = "SurveySystem",
                        IsActive = true,
                        InvitationId = invitationId,
                        QuestionId = question.QuestionId,
                        Order = index,
                        AnswerId = null, // пока что ответ - пустой
                        UserAnswerText = null,
                        IsValid = false
                    };
                    Database.UserAnswers.Create(saveModelAnsw);
                    index++;
                }

                var resA = await Database.Save();
                if (resA > 0)
                {
                    loggingService.Info($"UserAnswer for {invitation.UserEmail} auto created for survey {invitation.SurveyId}");
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateUserAnswer");
                }
                else
                {
                    loggingService.Warn("UserAnswer in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateUserAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateUserAnswer");
            }
        }

    public async Task<OperationDetails> CreateAsync(UserAnswerDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("UserAnswerDTO is Empty");
                    return new OperationDetails(false, "Ответ не найден / UserAnswer is Empty", string.Empty);
                }
                if (model.UserAnswerId == Guid.Empty || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false,
                        "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.QuestionId == Guid.Empty)
                {
                    loggingService.Error("User is not filled in");
                    return new OperationDetails(false, "Отсутствует id вопроса / Question  id is empty", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false,
                        "Значение даты создания недействительно / CreatedOn value is invalid", string.Empty);
                }

                var duplicateId = await Database.UserAnswers.GetAsync(model.UserAnswerId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in UserAnswerId = {model.UserAnswerId}");
                    return new OperationDetails(false, "Ответ с таким Id уже существует / Duplicate by Id ",
                        "Duplicate");
                }

                // check invitation
                var inv = await Database.Invitations.GetAsync(model.InvitationId);
                if (inv == null)
                {
                    loggingService.Error("Invitation not found");
                    return new OperationDetails(false, "Приглашение не найдено / Invitation not found", "Duplicate");
                }

                var saveModel = new UserAnswer
                {
                    UserAnswerId = model.UserAnswerId,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    QuestionId = model.QuestionId,
                    AnswerId = model.AnswerId,
                    UserAnswerText = !string.IsNullOrEmpty(model.UserAnswerText) ? model.UserAnswerText.Trim() : null,
                    IsValid = false
                };
                Database.UserAnswers.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateUserAnswer");
                }
                else
                {
                    loggingService.Warn("UserAnswer in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateUserAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateUserAnswer");
            }
        }
        
        public async Task<OperationDetails> UpdateAsync(UserAnswerDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("UserAnswerDTO is Empty");
                    return new OperationDetails(false, "Ответ не найден / UserAnswer is Empty", string.Empty);
                }
                if (model.QuestionId == Guid.Empty)
                {
                    loggingService.Error("User is not filled in");
                    return new OperationDetails(false, "Отсутствует id вопроса / Question  id is empty", string.Empty);
                }

                if (model.UserAnswerId == Guid.Empty)
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                var useranswer = await Database.UserAnswers.GetAsync(model.UserAnswerId);
                if (useranswer == null)
                {
                    loggingService.Error($"Not found = {model.UserAnswerId}");
                    return new OperationDetails(false, "Не найдено / Not found", "Duplicate");
                }
                if (useranswer.AnswerId == null)
                {
                    useranswer.AnswerId = Guid.Empty;
                }
                if (useranswer.AnswerId != Guid.Empty)
                {
                    loggingService.Error("User has answered this question");
                    return new OperationDetails(false, "Пользователь уже отвечал на этот вопрос / User has answered this question", string.Empty);
                }
                // check answer
                if (model.AnswerId != null)
                {
                    var answerDb = await Database.Answers.GetAsync((Guid)model.AnswerId);
                    if (answerDb == null)
                    {
                        loggingService.Error("answer not found");
                        return new OperationDetails(false, "Ответ не найден / Answer not found", "Duplicate");
                    }
                    loggingService.Warn($"Update: UserAnswer:{model.AnswerId}");
                    //Update
                   
                    useranswer.AnswerId = model.AnswerId;
                    useranswer.IsValid = answerDb.IsValid;
                    useranswer.UserAnswerText = string.Concat(answerDb.AnswerRus," / ", answerDb.AnswerEng);
                }
                else useranswer.UserAnswerText = !string.IsNullOrEmpty(model.UserAnswerText) ? model.UserAnswerText.Trim() : null;

                useranswer.IsActive = model.IsActive;
                
               
                Database.UserAnswers.Update(useranswer);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateUserAnswer");
                }
                else
                {
                    loggingService.Error("UpdateUserAnswer error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateUserAnswer");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateUserAnswer");
            }
        }

        public async Task<OperationDetails> UpdateFixedAsync(IEnumerable<UserAnswerDTO> modellist)
        {
            try
            {
                if (modellist == null)
                {
                    loggingService.Error("UserAnswer Modellist is Empty");
                    return new OperationDetails(false, "Нет данных для сохранения / No data to save", string.Empty);
                }

                // проверяем, что InvitationId одинаковое check invitation GENERAL
                bool sameInvitation = (modellist.Select(s => s.InvitationId).Distinct().Count() < 2);
                if (!sameInvitation)
                {
                    loggingService.Error("Different invitation of List");
                    return new OperationDetails(false, "Приглашение не действительно / Invitation invalid", string.Empty);
                }
                foreach (var model in modellist)
                {
                    if (model.QuestionId == Guid.Empty)
                    {
                        loggingService.Error("Question Id is empty");
                        return new OperationDetails(false, "Отсутствует id вопроса / Question Id is empty", string.Empty);
                    }
                    // check answer
                    if (model.AnswerId != null)
                    {
                        var answerDb = await Database.Answers.GetAsync((Guid)model.AnswerId);
                        if (answerDb == null)
                        {
                            loggingService.Error("answer not found");
                            return new OperationDetails(false, "Ответ не найден / Answer not found", "Duplicate");
                        }

                        var useranswer = (await Database.UserAnswers.FindAsync(x => x.QuestionId == model.QuestionId && x.InvitationId == model.InvitationId)).FirstOrDefault();
                        if (useranswer == null)
                        {
                            loggingService.Error("useranswer not found");
                            continue;
                            // return new OperationDetails(false, "Ответ не найден / Answer not found", "Duplicate");
                        }
                        if (useranswer.AnswerId != null)
                        {
                            loggingService.Warn("User has answered this question");
                            continue;
                            // return new OperationDetails(false, "Пользователь уже отвечал на этот вопрос / User has answered this question", string.Empty);
                        }

                        loggingService.Warn($"Update: UserAnswer:{model.InvitationId}");
                        //Update
                        useranswer.AnswerId = model.AnswerId;
                        useranswer.IsActive = model.IsActive;
                        useranswer.IsValid = answerDb.IsValid;
                        useranswer.UserAnswerText = string.Concat(answerDb.AnswerRus, " / ", answerDb.AnswerEng);
                        Database.UserAnswers.Update(useranswer);
                    }
                }

                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateFixedAsync");
                }
                else
                {
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateFixedAsync");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "UpdateFixedAsync");
            }
        }
    }
}