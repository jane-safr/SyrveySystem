using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Microsoft.AspNet.Identity;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("notification")]
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ILoggerService<NotificationController> loggingService;
        private readonly INotificationService notifyService;

        public NotificationController(ILoggerService<NotificationController> logServ, INotificationService notServ)
        {
            this.loggingService = logServ;
            this.notifyService = notServ;
        }
        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Notification");
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
                var resDto = (await notifyService.FindByFilterAsync(filter)).ToList();
                var mapper = MapperConfigVm.MapperConfigNotification();
                var notifications = mapper.Map<IEnumerable<NotificationDTO>, List<NotificationVM>>(resDto);
                return new JsonNetResult(new { success = true, data = notifications });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetAll Notification: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("notify")]
        public async Task<JsonNetResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("InvitId or testId - Empty");
                    return JsonNetResult.Warn("InvitId - Empty");
                }
                if (!this.User.IsInRole("admin") && !this.User.IsInRole("manager"))
                {
                    return new JsonNetResult(new { success = true, data = new List<NotificationVM>() });
                }
                var notifys = await notifyService.GetNotificationById(id);
                var mapper = MapperConfigVm.MapperConfigNotification();
                var resVm = mapper.Map<IEnumerable<NotificationDTO>, List<NotificationVM>>(notifys);
                return new JsonNetResult(new { success = true, data = resVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetById: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("notify/task")]
        public async Task<JsonNetResult> GetById(int tsc)
        {
            try
            {
                if (tsc < 1)
                {
                    loggingService.Error("taskCode - Empty");
                    return JsonNetResult.Warn("task - Empty");
                }
                if (!this.User.IsInRole("admin") && !this.User.IsInRole("manager"))
                {
                    return new JsonNetResult(new { success = true, data = new List<NotificationVM>() });
                }
                var notify = await notifyService.GetNotificationBySurveyCode(tsc);
                var mapper = MapperConfigVm.MapperConfigNotification();
                var resVm = mapper.Map<IEnumerable<NotificationDTO>, List<NotificationVM>>(notify);
                return new JsonNetResult(new { success = true, data = resVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetByCode: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("cancel")]
        public async Task<JsonNetResult> Delete(Guid notifyId)
        {
            try
            {
                if (notifyId == Guid.Empty)
                {
                    return JsonNetResult.Warn("Empty ID");
                }
                if (this.User.IsInRole("admin") || this.User.IsInRole("manager"))
                {
                    var user = User.Identity.GetUserName();
                    loggingService.Warn($"{user} Cancel - Notyfy {notifyId}");
                    var cancel = await notifyService.CancelAsync(notifyId);
                    return new JsonNetResult(new { success = cancel.Succedeed, message = cancel.Message });
                }
                return JsonNetResult.Failure("Доступ запрещен / Access denied");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex);
                return JsonNetResult.Failure(ex.Message);
            }
        }
    }
}