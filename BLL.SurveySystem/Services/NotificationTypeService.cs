using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
        public class NotificationTypeService : INotificationTypeService
        {
            IUnitOfWork Database { get; set; }
            readonly ILoggerService<NotificationTypeService> loggingService;
            public NotificationTypeService(IUnitOfWork uow, ILoggerService<NotificationTypeService> logServ)
            {
                this.Database = uow;
                this.loggingService = logServ;
            }

            public async Task<OperationDetails> CreateAsync(NotificationTypeDTO model)
            {
                try
                {
                    if (model == null)
                    {
                        loggingService.Error("NotificationTypeDTO is Empty");
                        return new OperationDetails(false, "Модель отсутствует / NotificationType is Empty", string.Empty);
                    }

                    if (model.NotificationTypeId == Guid.Empty || string.IsNullOrWhiteSpace(model.NameRus) || string.IsNullOrWhiteSpace(model.NameEng) || string.IsNullOrEmpty(model.CreatedBy))
                    {
                        loggingService.Error("Not all required fields are filled in");
                        return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                    }
                    if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                    {
                        loggingService.Error("CreatedOn value is not valid");
                        return new OperationDetails(false, "Значение даты создания не действительно / CreatedOn value is not valid", string.Empty);
                    }
                    if (string.IsNullOrWhiteSpace(model.MessageTemplate) && !HelperBll.ValidateText(model.MessageTemplate))
                    {
                        loggingService.Error("Enter data EmailTemplate");
                        return new OperationDetails(false, "Введите шаблон текста email / Enter message email template", string.Empty);
                    }
                    //TemplateLink
                    if (!string.IsNullOrWhiteSpace(model.TemplateLink) && !HelperBll.ValidateUrlNotificationType(model.TemplateLink))
                    {
                        loggingService.Error("Template link is not valid");
                        return new OperationDetails(false, "Значение ссылки недействительно / Template link is not valid", string.Empty);
                    }
                    
                    // только 1 тип
                    var onetypeSel =
                        (from prop in model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         where
                         prop.PropertyType == typeof(bool)
                         && prop.Name != "IsActive"
                         select new { Name = prop.Name, Value = (bool)prop.GetValue(model) }).Distinct().OrderBy(s => s.Name).ToList();
                    if (!onetypeSel.Any() || onetypeSel.Count(b => b.Value) > 1)
                    {
                        loggingService.Error("Please specify one type");
                        return new OperationDetails(false, "Укажите один тип / Please specify one type", string.Empty);
                    }
                    var valTrue = onetypeSel.FirstOrDefault(i => i.Value);
                    if (string.IsNullOrWhiteSpace(valTrue?.Name))
                    {
                        loggingService.Error("Specify at least one type");
                        return new OperationDetails(false, "Укажите хотя бы один тип / Specify at least one type", string.Empty);
                    }
                    var allItems = (await Database.NotificationTypes.FindAsync(i => i.NotificationTypeId != model.NotificationTypeId)).ToList();
                    foreach (var item in allItems)
                    {
                        if (item != null)
                        {
                            var allBoolItem = (from prop in item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                               where
                                                   prop.PropertyType == typeof(bool)
                                                   && prop.Name != "IsActive"
                                                   && prop.Name == valTrue.Name
                                               select new { Name = prop.Name, Value = (bool)prop.GetValue(item) }).Distinct().ToList();
                            if (allBoolItem.Any(vt => vt.Value))
                            {
                                loggingService.Error($"{valTrue.Name} exists true");
                                return new OperationDetails(false, $"{valTrue.Name} exists true", string.Empty);
                            }
                        }
                    }
                    var saveModel = new NotificationType
                    {
                        NotificationTypeId = model.NotificationTypeId,
                        CreatedOn = model.CreatedOn,
                        CreatedBy = model.CreatedBy,
                        IsActive = model.IsActive,
                        NameRus = HelperBll.DeleteRowTabToText(model.NameRus.Trim()),
                        NameEng = HelperBll.DeleteRowTabToText(model.NameEng.Trim()),
                        MessageTemplate = model.MessageTemplate.Trim(),
                        TemplateLink = string.IsNullOrEmpty(model.TemplateLink) ? null : model.TemplateLink.Trim(),
                        IsAnonymousSurvey = model.IsAnonymousSurvey,
                        IsSurvey = model.IsSurvey,
                        IsTest = model.IsTest

                    };
                    Database.NotificationTypes.Create(saveModel);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно добавлено / Create Completed", "CreateNotificationType");
                    }
                    else
                    {
                        loggingService.Warn("Create NotificationType in database None");
                        return new OperationDetails(false, "Не добавлено / Create Not Completed", "CreateNotificationType");
                    }
                }
                catch (Exception ex)
                {
                    loggingService.Error(ex.Message);
                    return new OperationDetails(false, "Не добавлено / Not Completed", "CreateNotificationType");
                }
            }

            public async Task<OperationDetails> DeleteAsync(Guid id)
            {
                try
                {
                    if (id == Guid.Empty)
                    {
                        loggingService.Error("Cancel notify Empty id");
                        return new OperationDetails(false, "Cancel notify Empty id", string.Empty);
                    }
                    var notify = await Database.NotificationTypes.GetAsync(id);
                    if (notify == null)
                    {
                        loggingService.Error("Notify type not found");
                        return new OperationDetails(false, "Тип не найден / Type not found", string.Empty);
                    }

                    if (!notify.IsActive)
                    {
                        loggingService.Error("Notify type no active");
                        return new OperationDetails(false, "Тип не активен / Type no active", string.Empty);
                    }

                    if (notify.Notifications.Any())
                    {
                        notify.IsActive = false;
                        Database.NotificationTypes.Update(notify);
                    }
                    else await Database.NotificationTypes.DeleteAsync(notify.NotificationTypeId);


                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Completed", "CancelNotificationType");
                    }
                    else
                    {
                        loggingService.Error("Update error");
                        return new OperationDetails(false, "Не удалено / Not Completed", "CancelNotificationType");
                    }
                }
                catch (Exception ex)
                {
                    loggingService.Error(ex.Message + " " + ex.StackTrace);
                    return new OperationDetails(false, $"{ex.Message}", "CancelNotificationType");
                }
            }

            public async Task<IEnumerable<NotificationTypeDTO>> GetAllAsync()
            {
                try
                {
                    var results = await Database.NotificationTypes.GetAllAsync();
                    var mapper = MapperAll.MapperConfigNotification();
                    var result = mapper.Map<IEnumerable<NotificationType>, List<NotificationTypeDTO>>(results);
                    return result;
                }
                catch (Exception ex)
                {
                    loggingService.Error(ex.Message + " " + ex.StackTrace);
                    return new List<NotificationTypeDTO>();
                }
            }

            public async Task<IEnumerable<NotificationTypeDTO>> FindByFilterAsync(List<FilterModels> filterModels)
            {
                try
                {
                    var inquiry = Database.NotificationTypes.GetIQueryable();
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
                                        inquiry = inquiry.Where(x => x.NotificationTypeId == idGuid);
                                    }
                                }
                            }
                        }
                    }
                    var fullData = await inquiry.Take(5000).AsNoTracking().ToListAsync();
                    var mapper = MapperAll.MapperConfigNotification();
                    var results = mapper.Map<IEnumerable<NotificationType>, List<NotificationTypeDTO>>(fullData);
                    return results;
                }
                catch (Exception ex)
                {
                    loggingService.Error(ex.Message + " " + ex.StackTrace);
                    return new List<NotificationTypeDTO>();
                }
            }

            public async Task<NotificationTypeDTO> GetByIdAsync(Guid id)
            {
                try
                {
                    if (id == Guid.Empty)
                    {
                        loggingService.Error("NotificationTypeId = Empty");
                        return new NotificationTypeDTO();
                    }
                    var dataFirst = await Database.NotificationTypes.GetAsync(id);
                    var mapper = MapperAll.MapperConfigNotification();
                    var item = mapper.Map<NotificationType, NotificationTypeDTO>(dataFirst);
                    return item;
                }
                catch (Exception ex)
                {
                    loggingService.Error(ex.Message + " " + ex.StackTrace);
                    return new NotificationTypeDTO();
                }
            }

            public async Task<OperationDetails> UpdateNotificationType(NotificationTypeDTO model)
            {
                try
                {
                    if (model == null)
                    {
                        loggingService.Error("NotificationType Model is Empty");
                        return new OperationDetails(false, "Модель отсутствует / Model is Empty", string.Empty);
                    }
                    if (model.NotificationTypeId == Guid.Empty)
                    {
                        loggingService.Error("NotificationTypeId is empty");
                        return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                    }
                    if (!string.IsNullOrWhiteSpace(model.NameRus) && !HelperBll.ValidateText(model.NameRus))
                    {
                        loggingService.Error("Notification Type Rus is not valid");
                        return new OperationDetails(false, "Наименование Rus Типа недействительно / Notification Type Rus is not valid", string.Empty);
                    }
                    if (!string.IsNullOrWhiteSpace(model.NameRus) && !HelperBll.ValidateText(model.NameEng))
                    {
                        loggingService.Error("Notification Type Eng is not valid");
                        return new OperationDetails(false, "Наименование Eng Типа недействительно / Notification Type Eng is not valid", string.Empty);
                    }

                    if (string.IsNullOrWhiteSpace(model.MessageTemplate) && !HelperBll.ValidateText(model.MessageTemplate))
                    {
                        loggingService.Error("template is not valid");
                        return new OperationDetails(false, "Шаблон письма недействителен / template is not valid", string.Empty);
                    }
                    //TemplateLink
                    if (!string.IsNullOrWhiteSpace(model.TemplateLink) && !HelperBll.ValidateUrlNotificationType(model.TemplateLink))
                    {
                        loggingService.Error("Template link is not valid");
                        return new OperationDetails(false, "Значение ссылки недействительно / Template link is not valid", string.Empty);
                    }
                   
                    var onetypeSel =
                        (from prop in model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         where
                             prop.PropertyType == typeof(bool)
                             && prop.Name != "IsActive"
                         select new { Name = prop.Name, Value = (bool)prop.GetValue(model) }).Distinct().OrderBy(s => s.Name).ToList();
                    if (!onetypeSel.Any() || onetypeSel.Count(b => b.Value) > 1)
                    {
                        loggingService.Error("Please specify one type");
                        return new OperationDetails(false, "Укажите один тип / Please specify one type", string.Empty);
                    }
                    var valTrue = onetypeSel.FirstOrDefault(i => i.Value);
                    if (string.IsNullOrWhiteSpace(valTrue?.Name))
                    {
                        loggingService.Error("Specify at least one type");
                        return new OperationDetails(false, "Укажите хотя бы один тип / Specify at least one type", string.Empty);
                    }
                    var allItems = (await Database.NotificationTypes.FindAsync(i => i.NotificationTypeId != model.NotificationTypeId)).ToList();
                    foreach (var item in allItems)
                    {
                        if (item != null)
                        {
                            var allBoolItem = (from prop in item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                               where
                                                   prop.PropertyType == typeof(bool)
                                                   && prop.Name != "IsActive"
                                                   && prop.Name == valTrue.Name
                                               select new { Name = prop.Name, Value = (bool)prop.GetValue(item) }).Distinct().ToList();
                            if (allBoolItem.Any(vt => vt.Value))
                            {
                                loggingService.Error($"{valTrue.Name} exists true");
                                return new OperationDetails(false, $"{valTrue.Name} exists true", string.Empty);
                            }
                        }
                    }

                    var type = await Database.NotificationTypes.GetAsync(model.NotificationTypeId);
                    if (type == null)
                    {
                        loggingService.Error("Data not found");
                        return new OperationDetails(false, " Данные не найдены / Data not found", "Duplicate");
                    }
                    loggingService.Warn($"Update: NameOld:{type.NameRus} -> NameNew:{model.NameRus}");
                    loggingService.Warn($"Update: NameOld:{type.NameEng} -> NameNew:{model.NameEng}");
                    //Update
                    type.NameRus = model.NameRus.Trim();
                    type.NameEng = model.NameEng.Trim();
                    type.IsActive = model.IsActive;
                    
                    //Шаблон
                    type.MessageTemplate = model.MessageTemplate.Trim();
                    type.IsTest = model.IsTest;
                    type.IsSurvey = model.IsSurvey;
                    type.IsAnonymousSurvey = model.IsAnonymousSurvey;
                    //tempLinnk
                    type.TemplateLink = !string.IsNullOrWhiteSpace(model.TemplateLink) ? model.TemplateLink.Trim() : null;
                    Database.NotificationTypes.Update(type);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateNotificationType");
                    }
                    else
                    {
                        loggingService.Error("Notification type not update to database");
                        return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateNotificationType");
                    }
                }
                catch (Exception ex)
                {
                    loggingService.Error(ex.Message + " " + ex.StackTrace);
                    return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateNotificationType");
                }
            }
        }
    }