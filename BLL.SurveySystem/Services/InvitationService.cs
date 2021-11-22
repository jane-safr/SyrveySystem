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
    public class InvitationService : IInvitationService
    {
        IUnitOfWork Database { get; set; }
        ILoggerService<InvitationService> loggingService;
        public InvitationService(IUnitOfWork uow, ILoggerService<InvitationService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<IEnumerable<InvitationDTO>> GetAllAsync()
        {
            try
            {
                var results = await Database.Invitations.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Invitation>, List<InvitationDTO>>(results);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<InvitationDTO>();
            }
        }

        public async Task<InvitationDTO> GetByCodeAsync(int codeId)
        {
            try
            {
                if (codeId < 1)
                {
                    loggingService.Error("codeId invalid");
                    return null;
                }
                var invitation = (await Database.Invitations.FindAsync(b => b.InvitationCode == codeId)).FirstOrDefault();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<Invitation, InvitationDTO>(invitation);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new InvitationDTO();
            }
        }

        public async Task<IEnumerable<InvitationDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Invitations.GetIQueryable();
                var inquiryAnon = inquiry;
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
                                inquiry = inquiry.Where(x => x.UserName.ToLower().Contains(filter.Value.ToLower().Trim()) || x.UserEmail.ToLower().Contains(filter.Value.ToLower().Trim()) || x.Survey.NameRus.ToLower().Contains(filter.Value.ToLower().Trim()) || x.Survey.NameEng.ToLower().Contains(filter.Value.ToLower().Trim()) || x.Survey.SurveyCode.ToString().ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.InvitationId == idGuid);
                                }
                            }
                            //employeeId
                            else if (filter.Field.StartsWith("employeeId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var id))
                                {
                                    inquiry = inquiry.Where(x => x.UserId == id);
                                }
                            }
                            // IS Anon
                            else if (filter.Field.StartsWith("IsAnonSurvey", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    var anon = inquiryAnon.Where(x => x.Survey.IsAnonymous == val);
                                    inquiry = anon.Any() ? inquiry.Union(anon) : inquiry; // union Anon + userId
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.OrderBy(x=>x.DateEnd).Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Invitation>, List<InvitationDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<InvitationDTO>();
            }
        }

        public async Task<InvitationDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Empty id for filter");
                    return new InvitationDTO();
                }
                var dataFirst = await Database.Invitations.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var item = mapper.Map<Invitation, InvitationDTO>(dataFirst);
                return item;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new InvitationDTO();
            }
        }
        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty InvitationId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }
                var inv = await Database.Invitations.GetAsync(id);
                if (inv != null)
                {
                    // Delete Invit: удаляем только не подтвержденные
                    if (inv.IsAccepted)
                    {
                        loggingService.Warn("Invitation delete attempt");
                        return new OperationDetails(false, "Пользователь уже начал проходить тест. Удаление невозможно / User has already started a test",
                            string.Empty);
                    }
                    //var userAnsws = await Database.UserAnswers.FindAsync(x => x.InvitationId == id);
                    //if (userAnsws.Any())
                    //{
                    //    loggingService.Warn("Invitation delete attempt");
                    //    return new OperationDetails(false,
                    //        "Пользователь уже начал проходить тест. Удаление невозможно / User has already started a test",
                    //        string.Empty);
                    //}
                    await Database.Invitations.DeleteAsync(id);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteInvitation");
                    }
                    else
                    {
                        loggingService.Warn("DeleteInvitation in database None");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteInvitation");
                    }
                }
                loggingService.Warn("DeleteInvitation in database not found");
                return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteInvitation");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteInvitation");
            }
        }

        public async Task<OperationDetails> InvitationResendAsync(Guid invitationId, string username)
        {
            try
            {
                if (invitationId == Guid.Empty)
                {
                    return new OperationDetails(false, "Идентификатор недействителен / Invalid identifier",
                        string.Empty);
                }

                if (string.IsNullOrEmpty(username))
                {
                    loggingService.Error("no username");
                    return new OperationDetails(false, "Пользователь недействителен / Invalid user", string.Empty);
                }

                var invitation = await Database.Invitations.GetAsync(invitationId);
                if (invitation == null)
                {
                    loggingService.Error("Invitation not found");
                    return new OperationDetails(false, "Приглашение не найдено / Invitation not found", string.Empty);
                }

                if (!invitation.IsActive)
                {
                    return new OperationDetails(false, "Приглашение не активно / Invitation not active", string.Empty);
                }

                if (invitation.IsFinished)
                {
                    return new OperationDetails(false, "Тест пройден / Survey was completed", string.Empty);
                }

                if (invitation.UserId == Guid.Empty)
                {
                    return new OperationDetails(false,
                        "Пользователь не определен / User not found", string.Empty);
                }
                if (invitation.DateEnd <= DateTime.Now)
                {
                    return new OperationDetails(false,
                        "Срок прохождения окончен / Deadline skipped", string.Empty);
                }
                // check User
                var user = await Database.UserManager.FindByIdAsync(invitation.UserId.ToString());
                if (user == null)
                {
                    loggingService.Error("User not found in db"); // нет в БД
                    // return new OperationDetails(false, "Пользователь не найден / User not found", "Duplicate");
                }

                if (string.IsNullOrEmpty(invitation.UserEmail) || string.IsNullOrEmpty(invitation.UserName) ||
                    invitation.UserId == Guid.Empty || !HelperBll.ValidateMail(invitation.UserEmail))
                {
                    loggingService.Error("User invalid");
                    return new OperationDetails(false, "Информация о пользователе недоступна / User invalid",
                        "Duplicate");
                }

                // check Survey
                var survey = await Database.Surveys.GetAsync(invitation.SurveyId);
                if (survey == null)
                {
                    loggingService.Error("survey not found");
                    return new OperationDetails(false, "Тест не найден / Survey not found", "Duplicate");
                }

                // добавляем уведомление
                var notifyType = (await Database.NotificationTypes.FindAsync(nt => nt.IsActive && nt.IsAnonymousSurvey))
                    .FirstOrDefault(); // TODO
                if (notifyType == null)
                {
                    loggingService.Error("Notification type not found");
                    return new OperationDetails(false, "Уведомление не найдено / Notification type not found",
                        string.Empty);
                }

                if (string.IsNullOrWhiteSpace(notifyType.MessageTemplate))
                {
                    loggingService.Error("Notification type MessageTemplate empty");
                    return new OperationDetails(false, "Шаблон уведомления не найден / Notification type not found",
                        string.Empty);
                }
                var baseUrl = (await Database.Settings.GetNameAsync("BaseUrl")).Value;
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    loggingService.Error("BaseUrl not found");
                    return new OperationDetails(false, "BaseUrl not found", "Resend");
                }

                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    CreatedOn = DateTime.Now,
                    CreatedBy = username,
                    EmailTo = invitation.UserEmail.Trim(),
                    Id = invitationId,
                    IsSend = false,
                    DateSend = null,
                    IsActive = true,
                    EmailText = notifyType.MessageTemplate
                        .Replace("#=SurveyCode#", invitation.Survey.SurveyCode.ToString())
                        .Replace("#=DateEnd#", invitation.DateEnd.ToString("dd.MM.yyyy HH:mm")).Trim(),
                    EmailUrl = notifyType.TemplateLink.Replace("#=BaseUrl#", baseUrl.Remove(baseUrl.LastIndexOf("/", StringComparison.Ordinal))).Replace("#=SurveyCode#", survey.SurveyCode.ToString()),
                    NotificationTypeId = notifyType.NotificationTypeId
                };
                //Add Notify
                Database.Notifications.Create(notification);
                var res = await Database.Save();

                if (res > 0)
                {
                    return new OperationDetails(true, "Приглашение отправлено повторно / Invitation was resent", "CreateInvitation");
                }
                else
                {
                    loggingService.Error("ResendCreateInvitation error");
                    return new OperationDetails(false, "Не отправлено / Not send", "CreateInvitation");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "CreateInvitation");
            }
        }

        public async Task<IEnumerable<InvitationDTO>> GetBySurveyCodeAsync(int codeSurvey)
        {
            try
            {
                if (codeSurvey < 1)
                {
                    loggingService.Error("codeSurvey invalid");
                    return new List<InvitationDTO>();
                }

                var survey = (await Database.Surveys.FindAsync(x => x.SurveyCode == codeSurvey)).FirstOrDefault();
                if (survey == null)
                {
                    loggingService.Error("Survey not found by code");
                    return new List<InvitationDTO>();
                }

                var invitations = await Database.Invitations.FindAsync(n => n.SurveyId == survey.SurveyId);
                var mapper = MapperAll.MapperConfigDto();
                var results = mapper.Map<IEnumerable<Invitation>, List<InvitationDTO>>(invitations);
                return results;

            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<InvitationDTO>();
            }
        }

        // 1. при обновлении учитываются время старта и isAccepted
        // 2. учитываются время завершения и isfinished
        public async Task<OperationDetails> UpdateAsync(InvitationDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("InvitationDTO is Empty");
                    return new OperationDetails(false, "Индикатор не найден / Invitation is empty", string.Empty);
                }
                if (model.InvitationId == Guid.Empty)
                {
                    loggingService.Error("InvitationId is empty");
                    return new OperationDetails(false, "Id индикатора отсутствует / Invitation Id is empty", string.Empty);
                }
                if (model.UserId == Guid.Empty || string.IsNullOrEmpty(model.UserName))
                {
                    loggingService.Error("User is not filled in");
                    return new OperationDetails(false, "Отсутствует сотрудник / User is not filled in", string.Empty);
                }
                if (model.DateStart == DateTime.MaxValue || model.DateStart == DateTime.MinValue || model.DateStart > model.DateEnd || model.DateEnd < DateTime.Now)
                {
                    loggingService.Error("DateEnd value is not valid");
                    return new OperationDetails(false, "Значение даты начала недействительно /  DateStart value is invalid", string.Empty);
                }
                if (model.DateEnd == DateTime.MaxValue || model.DateEnd == DateTime.MinValue)
                {
                    loggingService.Error("DateEnd value is not valid");
                    return new OperationDetails(false, "Значение даты окончания недействительно / Date End value is invalid", string.Empty);
                }

               /* if (model.Percent == 0)
                {
                    var allQuest = await Database.UserAnswers.FindAsync(x => x.InvitationId == model.InvitationId);
                    var answQuest = allQuest.Where(x => x.AnswerId != null || !string.IsNullOrEmpty(x.UserAnswerText));
                    if (allQuest.Count() != 0)
                    {
                        model.Percent = answQuest.Count() / allQuest.Count() * 100;
                    }
                }*/

                if (model.Percent < 0 || model.Percent > 100)
                {
                    loggingService.Error("Invalid percent");
                    return new OperationDetails(false, "Процент прохождения недействителен / Percent invalid", string.Empty);
                }

                if (model.IsFinished && (model.ActualCompleteDate == DateTime.MaxValue || model.ActualCompleteDate == DateTime.MinValue))
                {
                    loggingService.Error("Invalid complete date");
                    return new OperationDetails(false, "Значение факт.даты окончания недействительно / Complete date value is invalid", string.Empty);
                }
                // check User
            /*    var user = await Database.UserManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                {
                    loggingService.Error("User not found");
                    return new OperationDetails(false, "Пользователь не найден / User not found", "Duplicate");
                } 
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Id))
                {
                    loggingService.Error("User invalid");
                    return new OperationDetails(false, "Информация о пользователе недоступна / User invalid", "Duplicate");
                }*/
                // check Survey
                var survey = await Database.Surveys.GetAsync(model.SurveyId);
                if (survey == null)
                {
                    loggingService.Error("survey not found");
                    return new OperationDetails(false, "Тест не найден / Survey not found", "Duplicate");
                }
                var invit = await Database.Invitations.GetAsync(model.InvitationId);
                if (invit == null)
                {
                    loggingService.Error($"InvitationId = {model.InvitationId} not found");
                    return new OperationDetails(false, "Приглашение не найдено / Invitation not found ", string.Empty);
                }
                if (invit.IsFinished)
                {
                    loggingService.Error($"InvitationId = {model.InvitationId} not found");
                    return new OperationDetails(false, "Тест уже был пройден / test completed", string.Empty);
                }
                if (invit.DateStart > model.ActualCompleteDate)
                {
                    loggingService.Error("DateStart > ActualCompleteDate");
                    return new OperationDetails(false, "Тест уже был пройден / ActualCompleteDate invalid", string.Empty);
                }

                //Update
                invit.IsAccepted = model.IsAccepted;
                invit.IsActive = model.IsActive;
                invit.DateStart = model.DateStart;
                invit.IsFinished = model.IsFinished;
                invit.Percent = model.Percent;
                invit.ActualCompleteDate = model.ActualCompleteDate;

                Database.Invitations.Update(invit);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateInvitation");
                }
                else
                {
                    loggingService.Error("UpdateInvitation error");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateInvitation");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateInvitation");
            }
        }
        public async Task<OperationDetails> CreateMultipeAsync(List<InvitationDTO> model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("List InvitationDTO is Empty");
                    return new OperationDetails(false, "Приглашения не найдены / Invitations are Empty", string.Empty);
                }
                string message = string.Empty;
               
                var baseUrl = (await Database.Settings.GetNameAsync("BaseUrl")).Value;
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    loggingService.Error("BaseUrl not found");
                    return new OperationDetails(false, "BaseUrl not found", "CreateMultipeAsync");
                }

                foreach (var invitation in model)
                {
                    if (invitation.InvitationId == Guid.Empty || string.IsNullOrEmpty(invitation.CreatedBy))
                    {
                        loggingService.Error("Not all required fields are filled in");
                        continue;
                        //return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                    }

                    if (invitation.InvitationCode < 1)
                    {
                        loggingService.Error("Error InvitationCode < 1");
                        continue;
                    }

                    if (invitation.UserId == Guid.Empty || string.IsNullOrEmpty(invitation.UserName))
                    {
                        loggingService.Error("User is not filled in");
                        continue;
                        //return new OperationDetails(false, "Отсутствует сотрудник / User is not filled in", string.Empty);
                    }

                    if (invitation.CreatedOn == DateTime.MaxValue || invitation.CreatedOn == DateTime.MinValue)
                    {
                        loggingService.Error("CreatedOn value is not valid");
                        continue;
                        //return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is invalid", string.Empty);
                    }

                    if (invitation.DateEnd == DateTime.MaxValue || invitation.DateEnd == DateTime.MinValue ||
                        invitation.DateEnd < DateTime.Now)
                    {
                        loggingService.Error("DateEnd value is not valid");
                        continue;
                        // return new OperationDetails(false, "Значение даты окончания недействительно / Date End value is invalid", string.Empty);
                    }

                    var duplicateId = await Database.Invitations.GetAsync(invitation.InvitationId);
                    if (duplicateId != null)
                    {
                        loggingService.Error($"Duplicate in InvitationId = {invitation.InvitationId}");
                        continue;
                        //return new OperationDetails(false, "Критерий с таким Id уже существует / Duplicate by Id ", "Duplicate");
                    }

                    if (string.IsNullOrEmpty(invitation.UserEmail) || string.IsNullOrEmpty(invitation.UserName) ||
                        invitation.UserId == Guid.Empty || !HelperBll.ValidateMail(invitation.UserEmail))
                    {
                        loggingService.Error("User invalid");
                        continue;
                        // return new OperationDetails(false, "Информация о пользователе недоступна / User invalid", "Duplicate");
                    }

                    // check Survey
                    var survey = await Database.Surveys.GetAsync(invitation.SurveyId);
                    if (survey == null)
                    {
                        loggingService.Error("survey not found");
                        continue;
                        // return new OperationDetails(false, "Тест не найден / Survey not found", "Duplicate");
                    }

                    // duplicate user + survey - уже было приглашение для этого пользователя
                    var dupUserSurv = await Database.Invitations.FindAsync(x =>
                        x.UserId == invitation.UserId && x.SurveyId == invitation.SurveyId);
                    if (dupUserSurv.Any())
                    {
                        loggingService.Error($"Duplicate in User + Survey {invitation.UserName} {survey.SurveyCode}");
                        message +=
                            $"{invitation.UserName} уже приглашен на тест / {invitation.UserName} was already invited <br/>";
                        continue;
                        //return new OperationDetails(false, "Критерий с таким Id уже существует / Duplicate by Id ", "Duplicate");
                    }

                    var saveModel = new Invitation
                    {
                        InvitationId = invitation.InvitationId,
                        CreatedOn = invitation.CreatedOn,
                        CreatedBy = invitation.CreatedBy,
                        IsActive = invitation.IsActive,
                        UserId = invitation.UserId,
                        UserName = invitation.UserName.Trim(),
                        UserEmail = invitation.UserEmail.Trim(),
                        DateEnd = invitation.DateEnd,
                        DateStart = null,
                        InvitationCode = invitation.InvitationCode,
                        IsAccepted = false,
                        IsFinished = false,
                        SurveyId = invitation.SurveyId,
                        Percent = 0
                    };
                    Database.Invitations.Create(saveModel);
                    
                    // TODO IS ANON or NOT
                    var notifAnon = (await Database.NotificationTypes.FindAsync(x => x.IsAnonymousSurvey))
                        .FirstOrDefault();
                    if (notifAnon == null)
                    {
                        loggingService.Error("Notification type not found");
                        return new OperationDetails(false, "Уведомление не найдено / Notification type not found",
                            string.Empty);
                    }

                    //Уведомление пользователя пройти тест
                    var notifCreate = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        CreatedOn = DateTime.Now,
                        CreatedBy = invitation.CreatedBy,
                        EmailTo = invitation.UserEmail,
                        Id = invitation.UserId,
                        IsSend = false,
                        DateSend = null,
                        IsActive = true,
                        EmailText = notifAnon.MessageTemplate.Replace("#=SurveyCode#", survey.SurveyCode.ToString()).Replace("#=DateEnd#", invitation.DateEnd.ToString("g")).Trim(),
                        EmailUrl = notifAnon.TemplateLink.Replace("#=BaseUrl#", baseUrl.Remove(baseUrl.LastIndexOf("/", StringComparison.Ordinal))).Replace("#=SurveyCode#", invitation.InvitationId.ToString()),
                        NotificationTypeId = notifAnon.NotificationTypeId
                    };
                    //Add Notify
                    Database.Notifications.Create(notifCreate);
                }

                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Create Completed <br>" + message, "CreateInvitation");
                }
                else
                {
                    loggingService.Warn("Invitation in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed <br>" + message, "CreateInvitation");
                }
            }

            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Приглашения не добавлены / Not Completed", "CreateInvitation");
            }
        }


        public async Task<OperationDetails> CreateAnonAsync(int surveyCode)
        {
            // найти Survey
            var survey = (await Database.Surveys.FindAsync(x => x.SurveyCode == surveyCode)).FirstOrDefault();
            if (survey == null)
            {
                loggingService.Error("Invalid survey");
                return new OperationDetails(false, "survey not found", "Auto");
            }

            var setGuid = await Database.Settings.GetNameAsync("AnonymousGuid");
            if (string.IsNullOrEmpty(setGuid?.Value))
            {
                loggingService.Error("Invalid Settings.GetNameAsync(AnonymousGuid");
                return new OperationDetails(false, "Invalid Settings.GetNameAsync(AnonymousGuid)", "Auto");
            }

            if (!Guid.TryParse(setGuid.Value, out var anonymousGuid))
            {
                loggingService.Error("Invalid Guid Tryparse AnonymousGuid");
                return new OperationDetails(false, "Invalid Guid Tryparse AnonymousGuid", "Auto");
            }

            var newGuid = Guid.NewGuid();
            var saveModel = new Invitation
            {
                InvitationId = newGuid,
                InvitationCode = Math.Abs(newGuid.GetHashCode()),
                CreatedOn = DateTime.Now,
                CreatedBy = "Anonymous",
                IsActive = true,
                UserId = anonymousGuid,
                UserName = "Anonymous",
                UserEmail = "Anonymous",
                DateEnd = DateTime.Now.AddDays(1),
                IsAccepted = false,
                IsFinished = false,
                SurveyId = survey.SurveyId
            };
            Database.Invitations.Create(saveModel);
            var res = await Database.Save();
            if (res > 0)
            {
                return new OperationDetails(true, "Успешно добавлено / Create Completed", saveModel.InvitationCode.ToString());
            }
            else
            {
                loggingService.Warn("Invitation in database None");
                return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateInvitation");
            }
        }

        public async Task<OperationDetails> CreateAsync(InvitationDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("InvitationDTO is Empty");
                    return new OperationDetails(false, "Приглашение не найдено / Invitation is Empty", string.Empty);
                }

                if (model.InvitationId == Guid.Empty || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }

                if (model.InvitationCode < 1)
                {
                    loggingService.Error("InvitationCode invalid");
                    return new OperationDetails(false, "Неверный код / Invitation Code invalid", string.Empty);
                }

                if (model.UserId == Guid.Empty || string.IsNullOrEmpty(model.UserName))
                {
                    loggingService.Error("User is not filled in");
                    return new OperationDetails(false, "Отсутствует сотрудник / User is not filled in", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue || model.DateEnd < DateTime.Now)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is invalid", string.Empty);
                }
                if (model.DateEnd == DateTime.MaxValue || model.DateEnd == DateTime.MinValue)
                {
                    loggingService.Error("DateEnd value is not valid");
                    return new OperationDetails(false, "Значение даты окончания недействительно / Date End value is invalid", string.Empty);
                }
                var duplicateId = await Database.Invitations.GetAsync(model.InvitationId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in InvitationId = {model.InvitationId}");
                    return new OperationDetails(false, "Приглашение с таким Id уже существует / Duplicate by Id ", "Duplicate");
                }
                // check User
                var user = await Database.UserManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                {
                    loggingService.Error("User not found");
                    return new OperationDetails(false, "Пользователь не найден / User not found", "Duplicate");
                }
                if(string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Id))
                {
                    loggingService.Error("User invalid");
                    return new OperationDetails(false, "Информация о пользователе недоступна / User invalid", "Duplicate");
                }
                // check Survey
                var survey = await Database.Surveys.GetAsync(model.SurveyId);
                if (survey == null)
                {
                    loggingService.Error("survey not found");
                    return new OperationDetails(false, "Тест не найден / Survey not found", "Duplicate");
                }
                var baseUrl = (await Database.Settings.GetNameAsync("BaseUrl")).Value;
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    loggingService.Error("BaseUrl not found");
                    return new OperationDetails(false, "BaseUrl not found", "CreateMultipeAsync");
                }

                var saveModel = new Invitation
                {
                    InvitationId = model.InvitationId,
                    InvitationCode = model.InvitationCode,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    UserId = Guid.Parse(user.Id),
                    UserName = user.UserName.Trim(),
                    UserEmail = user.Email.Trim(),
                    DateEnd = model.DateEnd,
                    IsAccepted = false,
                    IsFinished = false,
                    SurveyId = model.SurveyId
                };
                Database.Invitations.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    // добавляем уведомление
                    var notifyType = (await Database.NotificationTypes.FindAsync(nt => nt.IsActive && nt.IsAnonymousSurvey)).FirstOrDefault(); // TODO
                    if (notifyType == null)
                    {
                        loggingService.Error("Notification type not found");
                        return new OperationDetails(false, "Уведомление не найдено / Notification type not found", string.Empty);
                    }
                    if (string.IsNullOrWhiteSpace(notifyType.MessageTemplate))
                    {
                        loggingService.Error("Notification type MessageTemplate empty");
                        return new OperationDetails(false, "Шаблон уведомления не найден / Notification type not found", string.Empty);
                    }
                    
                    var notification = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        CreatedOn = DateTime.Now,
                        CreatedBy = model.CreatedBy,
                        EmailTo = user.Email.Trim(),
                        Id = model.InvitationId,
                        IsSend = false,
                        DateSend = null,
                        IsActive = true,
                        EmailText = notifyType.MessageTemplate.Replace("#=SurveyCode#", model.Survey.SurveyCode.ToString()).Replace("#=DateEnd#", model.DateEnd.ToString("dd.MM.yyyy HH:mm")).Trim(),
                        EmailUrl = notifyType.TemplateLink.Replace("#=BaseUrl#", baseUrl.Remove(baseUrl.LastIndexOf("/", StringComparison.Ordinal))).Replace("#=SurveyCode#", model.InvitationId.ToString()),
                        NotificationTypeId = notifyType.NotificationTypeId
                    };
                    //Add Notify
                    Database.Notifications.Create(notification);
                    await Database.Save(); // TODO
                    return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateInvitation");
                }
                else
                {
                    loggingService.Warn("Invitation in database None");
                    return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateInvitation");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateInvitation");
            }
        }
    }
}