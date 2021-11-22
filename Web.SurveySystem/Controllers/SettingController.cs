using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Interfaces;
using Microsoft.AspNet.Identity;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("settings")]
    [Authorize]
    public class SettingController : Controller
    {
        private readonly ISettingService settingService;
        private readonly ILoggerService<SettingController> loggingService;

        public SettingController(ISettingService serv, ILoggerService<SettingController> loggingService)
        {
            this.settingService = serv;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public ActionResult Index()
        {
            if (!this.User.IsInRole("admin"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Settings");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [Route("all")]
        public async Task<JsonNetResult> GetSettings()
        {
            try
            {
                var settingsDtos = await settingService.GetSettings();
                var mapper = MapperConfigVm.MapperConfigSettings();
                var settings = mapper.Map<IEnumerable<SettingDTO>, List<SettingVM>>(settingsDtos);
                return new JsonNetResult(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                loggingService.Error($"GetSettingsError: {ex.Message}");
                return JsonNetResult.Failure(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [Route("update")]
        public async Task<JsonNetResult> UpdateSetting(SettingVM model)
        {
            try
            {
                if (model == null)
                {
                    return JsonNetResult.Failure("Пустая модель / Empty Model");
                }

                var user = User.Identity.GetUserName();
                loggingService.Info($"{user} UpdateSettingStart");
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c => { c.CreateMap<SettingVM, SettingDTO>(); });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var settingsDto = mapper.Map<SettingVM, SettingDTO>(model);
                    var update = await settingService.UpdateSetting(settingsDto);
                    if (update.Succedeed)
                    {
                        return new JsonNetResult(new { success = true, message = update.Message });
                    }
                    else
                    {
                        return new JsonNetResult(new { success = false, message = update.Message });
                    }
                }
                else
                {
                    var listErrors = string.Join(" | ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error($"{user}  Error: {listErrors}");
                    return JsonNetResult.Failure(listErrors);
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex);
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                Response.StatusDescription = ex.Message.Replace("\r\n", " | ");
                return JsonNetResult.Failure(ex.Message);
            }
        }

    }
}