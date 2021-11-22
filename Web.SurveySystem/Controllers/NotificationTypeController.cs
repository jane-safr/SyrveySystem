using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Microsoft.AspNet.Identity;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [Authorize]
    [RoutePrefix("notificationtype")]
    public class NotificationTypeController : Controller
    {
        readonly ILoggerService<NotificationTypeController> loggingService;
        readonly INotificationTypeService notificationTypeService;
        public NotificationTypeController(ILoggerService<NotificationTypeController> logService, INotificationTypeService typeService)
        {
            this.loggingService = logService;
            this.notificationTypeService = typeService;
        }

        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> notificationtype");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<JsonNetResult> GetAll(string searchtxt)
        {
            try
            {
                var filter = new List<FilterModels>();
                if (!string.IsNullOrEmpty(searchtxt))
                {
                    if (HelperVm.IsGuid(searchtxt))
                    {
                        filter.Add(new FilterModels
                        {
                            Field = "Id",
                            Value = searchtxt.Trim()
                        });
                    }
                    else
                    {
                        filter.Add(new FilterModels
                        {
                            Field = "Searchtxt",
                            Value = searchtxt.Trim()
                        });
                    }
                }
                var resDto = await notificationTypeService.FindByFilterAsync(filter);
                var mapper = MapperConfigVm.MapperConfigNotification();
                var resVm = mapper.Map<IEnumerable<NotificationTypeDTO>, List<NotificationTypeVM>>(resDto);
                return new JsonNetResult(new { success = true, data = resVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetAll: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("add/{notifyTypeId?}")]
        public async Task<ActionResult> CreateOrEdit(Guid? notifyTypeId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("manager"))
            {
                loggingService.Error("Доступ запрещен notifyTypeId");
                return new HttpStatusCodeResult(403);
            }
            if (notifyTypeId != null && notifyTypeId != Guid.Empty)
            {
                var notifyType = await notificationTypeService.GetByIdAsync((Guid)notifyTypeId);
                if (notifyType == null)
                {
                    loggingService.Error("notifyType not found");
                    return new HttpStatusCodeResult(404);
                }
                ViewBag.NotificationTypeId = notifyType.NotificationTypeId;
                return View();
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("type")]
        public async Task<JsonNetResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }
                var compos = await notificationTypeService.GetByIdAsync(id);
                var mapper = MapperConfigVm.MapperConfigNotification();
                var result = mapper.Map<NotificationTypeDTO, NotificationTypeVM>(compos);
                return new JsonNetResult(new { success = true, data = result });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("remove")]
        public async Task<JsonNetResult> DeleteNotifyAsync(Guid notifyId)
        {
            try
            {
                if (notifyId == Guid.Empty)
                {
                    return JsonNetResult.Warn("Empty notifyTypeId");
                }
                if (this.User.IsInRole("admin") || this.User.IsInRole("manager"))
                {
                    var user = User.Identity.GetUserName();
                    loggingService.Warn($"{user} Delete - NotificationType {notifyId}");
                    var deleted = await notificationTypeService.DeleteAsync(notifyId);
                    return new JsonNetResult(new { success = deleted.Succedeed, message = deleted.Message });
                }
                return JsonNetResult.Failure("Доступ запрещен / Access denied");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex);
                return JsonNetResult.Failure(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("add")]
        public async Task<JsonNetResult> Create(NotificationTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    //Create
                    if (model.NotificationTypeId == null || model.NotificationTypeId == Guid.Empty)
                    {
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<NotificationTypeVM, NotificationTypeDTO>()
                                .ForMember(x => x.NotificationTypeId, m => m.MapFrom(s => Guid.NewGuid()))
                                .ForMember(m => m.Notifications, map => map.Ignore())
                                .ForMember(x => x.IsActive, x => x.MapFrom(m => true))
                                .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                                .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user));
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelDto = mapper.Map<NotificationTypeVM, NotificationTypeDTO>(model);
                        var created = await notificationTypeService.CreateAsync(modelDto);
                        return new JsonNetResult(new { success = created.Succedeed, message = created.Message });
                    }
                    //update
                    else
                    {
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<NotificationTypeVM, NotificationTypeDTO>()
                                .ForMember(m => m.Notifications, map => map.Ignore());
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelDto = mapper.Map<NotificationTypeVM, NotificationTypeDTO>(model);
                        var update = await notificationTypeService.UpdateNotificationType(modelDto);
                        return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                    }
                }
                else
                {
                    var listErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error(listErrors);
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"Create Failed: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("active")]
        public async Task<JsonNetResult> GetAllActive(string searchtxt)
        {
            try
            {
                var filter = new List<FilterModels>
                {
                    new FilterModels
                    {
                        Field = "IsActive",
                        Value = "true"
                    }
                };
                if (!string.IsNullOrEmpty(searchtxt))
                {

                    if (HelperVm.IsGuid(searchtxt))
                    {
                        filter.Add(new FilterModels
                        {
                            Field = "Id",
                            Value = searchtxt.Trim()
                        });
                    }
                    else
                    {
                        filter.Add(new FilterModels
                        {
                            Field = "Searchtxt",
                            Value = searchtxt.Trim()
                        });
                    }
                }
                var resDto = await notificationTypeService.FindByFilterAsync(filter);
                var mapper = MapperConfigVm.MapperConfigNotification();
                var resVm = mapper.Map<IEnumerable<NotificationTypeDTO>, List<NotificationTypeVM>>(resDto);
                return new JsonNetResult(new { success = true, data = resVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetCounts: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }
    }
}