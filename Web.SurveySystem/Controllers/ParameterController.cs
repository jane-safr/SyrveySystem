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
    [RoutePrefix("parameter")]
    public class ParameterController : Controller
    {
        private readonly IParameterService paramService;
        private readonly ILoggerService<ParameterController> loggingService;

        public ParameterController(IParameterService paramService, ILoggerService<ParameterController> loggingService)
        {
            this.paramService = paramService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Parameter");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("edit/{id}/{tabId?}")]
        public async Task<ActionResult> ParameterDetails(Guid id, int? tabId = 0)
        {
            if (id == Guid.Empty)
                return RedirectToAction("NotFound", "Error");

            var param = await paramService.GetByIdAsync(id);
            if (param != null)
            {
                ViewBag.ParameterId = param.ParameterId;
                ViewBag.CriterionId = param.CriterionId;
                ViewBag.CurrentUserId = User.Identity.GetUserId();
                ViewBag.TabId = tabId > 0 ? tabId : 0;
            }
            else
            {
                return RedirectToAction("NotFound", "Error");
            }
            return View("Details");
        }

        [HttpGet]
        [Authorize]
        [Route("byid")]
        public async Task<JsonNetResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }
                var chart = await paramService.GetByIdAsync(id);
                var mapper = MapperConfigVm.MapperConfigAll();
                var parVm = mapper.Map<ParameterDTO, ParameterVM>(chart);
                return new JsonNetResult(new { success = true, data = parVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [Route("all")]
        public async Task<JsonNetResult> GetParameters()
        {
            try
            {
                var paramDto = await paramService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var par = mapper.Map<IEnumerable<ParameterDTO>, List<ParameterVM>>(paramDto);
                return new JsonNetResult(new {success = true, data = par });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
        [Route("active")]
        public async Task<JsonNetResult> GetActiveCriterions(List<FilterModels> filterModels)
        {
            try
            {
                var crit = await paramService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<ParameterDTO>, List<ParameterVM>>(crit);
                return new JsonNetResult(new {success = true, data = resVm});
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetActive: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("edit")]
        public async Task<JsonNetResult> Update(ParameterVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<ParameterVM, ParameterDTO>()
                            .ForMember(s => s.Indicators, map => map.Ignore())
                            .ForMember(s => s.Criterion, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<ParameterVM, ParameterDTO>(model);
                    var update = await paramService.UpdateAsync(modelDto);
                    return new JsonNetResult(new {success = update.Succedeed, message = update.Message});
                }
                else
                {
                    var listErrors = string.Join("<br/>",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error(listErrors);
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"Update: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("add")]
        public async Task<JsonNetResult> Create(ParameterVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<ParameterVM, ParameterDTO>()
                            .ForMember(x => x.ParameterId, m => m.MapFrom(s => Guid.NewGuid()))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.Indicators, map => map.Ignore())
                            .ForMember(s => s.Criterion, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<ParameterVM, ParameterDTO>(model);
                    var update = await paramService.CreateAsync(modelDto);
                    return new JsonNetResult(new {success = update.Succedeed, message = update.Message});
                }
                else
                {
                    var listErrors = string.Join("<br/>",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error(listErrors);
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"{e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("remove")]
        public async Task<JsonNetResult> Delete(Guid id)
        {
            try
            {
                var user = User.Identity.GetUserName();
                if (id == Guid.Empty)
                {
                    loggingService.Warn($"{user} Parameter Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Parameter Empty Id");
                }
                var deleted = await paramService.DeleteAsync(id);
                if (deleted.Succedeed)
                {
                    return JsonNetResult.SuccessMessage(deleted.Message);
                }
                else
                {
                    return JsonNetResult.Failure(deleted.Message);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"{e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }
    }
}