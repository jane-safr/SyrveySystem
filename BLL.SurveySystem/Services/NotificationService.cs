using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class NotificationService : INotificationService
    {
        private IUnitOfWork Database { get; set; }
        private readonly ILoggerService<NotificationService> loggingService;
        public NotificationService(IUnitOfWork uow, ILoggerService<NotificationService> logServ)
        {
            this.Database = uow;
            this.loggingService = logServ;
        }

        public async Task<OperationDetails> CancelAsync(Guid notifyId)
        {
            try
            {
                if (notifyId == Guid.Empty)
                {
                    loggingService.Error("NotifyId empty");
                    return new OperationDetails(false, "Идентификатор не действителен / ID is not valid", "CreateNotification");
                }
                //Уведомление
                var notify = await Database.Notifications.GetAsync(notifyId);
                if (notify == null)
                {
                    loggingService.Error($"NotifyId {notifyId} not found");
                    return new OperationDetails(false, "Уведомление не найдено / No notification found", "CancelNotification");
                }
                //Если отправлено
                if (notify.IsSend)
                {
                    loggingService.Error($"NotifyId {notifyId} Action not available");
                    return new OperationDetails(false, "Действие не доступно / Action not available", "CancelNotification");
                }
                //Если не активно
                if (!notify.IsActive)
                {
                    loggingService.Error($"NotifyId {notifyId} Notification not active");
                    return new OperationDetails(false, "Уведомление не активно / Notification not active", "CancelNotification");
                }
                notify.IsActive = false;
                Database.Notifications.Update(notify);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Successfully updated", "CreateNotification");
                }
                else
                {
                    return new OperationDetails(false, "Данные не обновлены / Data not updated", "CreateNotification");
                }

            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, "Внутренняя ошибка сервера / Internal Server Error ", "CancelNotification");
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetNotificationById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("Notify empty id");
                    return new List<NotificationDTO>();
                }
                var notifications = (await Database.Notifications.FindAsync(n => n.Id == id)).ToList();
                var mapper = MapperAll.MapperConfigNotification();
                var result = mapper.Map<IEnumerable<Notification>, List<NotificationDTO>>(notifications);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<NotificationDTO>();
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetNotificationBySurveyCode(int taskCode)
        {
            try
            { // TODO
                if (taskCode < 1)
                {
                    loggingService.Error("Notify empty id");
                    return new List<NotificationDTO>();
                }
                var task = (await Database.Surveys.FindAsync(t => t.SurveyCode == taskCode)).FirstOrDefault();
                if (task == null)
                {
                    loggingService.Error($"Survey not found code = {taskCode}");
                    return new List<NotificationDTO>();
                }
                var notifications = (await Database.Notifications.FindAsync(n => n.Id == task.SurveyId)).ToList();
                var mapper = MapperAll.MapperConfigNotification();
                var result = mapper.Map<IEnumerable<Notification>, List<NotificationDTO>>(notifications);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<NotificationDTO>();
            }
        }

        public async Task<OperationDetails> CreateAsync(NotificationDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("NotificationDTO Model is Empty");
                    return new OperationDetails(false, "Уведомление отсутствует / Notification is Empty", string.Empty);
                }
                if (model.NotificationId == Guid.Empty || string.IsNullOrEmpty(model.EmailTo))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }

                if (!HelperBll.ValidateMail(model.EmailTo))
                {
                    loggingService.Error("Email is not valid");
                    return new OperationDetails(false, "Email недействителен / Email is not valid", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("Creation date value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / Creation date value is not valid", string.Empty);
                }
                if (model.NotificationTypeId == Guid.Empty)
                {
                    loggingService.Error("Invalid NotificationTypeId");
                    return new OperationDetails(false, "Недействительный идентификатор типа / Invalid NotificationType Id", string.Empty);
                }

                var notif = await Database.Notifications.GetAsync(model.NotificationId);
                if (notif != null)
                {
                    loggingService.Error($"Duplicate in Notification by Id = {model.NotificationId}");
                    return new OperationDetails(false, "Дубликат / Duplicate by Id ", "Duplicate");
                }
                //Модель
                var saveModel = new Notification
                {
                    NotificationId = model.NotificationId,
                    DateSend = model.DateSend,
                   // EmailTo = HelpBLL.DeleteRowTabToText(model.EmailTo.Trim()),
                   // EmailText = HelpBLL.DeleteRowTabToText(model.EmailText.Trim()),
                    IsSend = model.IsSend,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    NotificationTypeId = model.NotificationTypeId,
                    EmailUrl = model.EmailUrl.Trim(),
                    Id = model.Id
                };
                Database.Notifications.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Completed", "CreateNotification");
                }
                else
                {
                    return new OperationDetails(false, "Ошибка при добавлении / Not Completed", "CreateNotification");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, "Внутренняя ошибка сервера / Internal Server Error ", "CreateNotification");
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetNotSentNotifications()
        {
            try
            {
                // список неотпр. сообщений из Notification
                var listNotifications = (await Database.Notifications.FindAsync(x => x.IsActive && x.IsSend == false)).ToList();
                if (!listNotifications.Any())
                {
                    loggingService.Info("No data to send");
                    return new List<NotificationDTO>();
                }
                var config = new MapperConfiguration(c => { c.CreateMap<Notification, NotificationDTO>(); });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var result = mapper.Map<IEnumerable<Notification>, List<NotificationDTO>>(listNotifications);
                return result;
            }
            catch (Exception e)
            {
                loggingService.Error($"Error: {e.Message}");
                return new List<NotificationDTO>();
            }
        }

        public async Task<IEnumerable<NotificationDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var query = Database.Notifications.GetIQueryable();
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
                                    query = query.Where(x => x.IsActive == val);
                                }
                            }
                            else if (filter.Field.StartsWith("Searchtxt", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                query = query.Where(x => x.EmailTo.ToLower().Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    query = query.Where(x => x.NotificationId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await query.Take(50).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigNotification();
                var results = mapper.Map<IEnumerable<Notification>, List<NotificationDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<NotificationDTO>();
            }
        }
    }
}
