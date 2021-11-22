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
    [RoutePrefix("criterion")]
    public class CriterionController : Controller
    {
        private readonly ICriterionService criterionService;
        private ILoggerService<CriterionController> loggingService;
        public CriterionController(ICriterionService criterionService, ILoggerService<CriterionController> loggingService)
        {
            this.criterionService = criterionService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Criterion");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Route("all")]
        public async Task<JsonNetResult> GetCriterions()
        {
            try
            {
                var critDto = await criterionService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var crit = mapper.Map<IEnumerable<CriterionDTO>, List<CriterionVM>>(critDto);
                return new JsonNetResult(new { success = true, data = crit });
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
                var crit = await criterionService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<CriterionDTO>, List<CriterionVM>>(crit);
                return new JsonNetResult(new { success = true, data = resVm });
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
        public async Task<JsonNetResult> Update(CriterionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<CriterionVM, CriterionDTO>()
                            .ForMember(s => s.Parameters, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<CriterionVM, CriterionDTO>(model);
                    var update = await criterionService.UpdateAsync(modelDto);
                    return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                }
                else
                {
                    var listErrors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
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
        public async Task<JsonNetResult> Create(CriterionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<CriterionVM, CriterionDTO>()
                            .ForMember(x => x.CriterionId, m => m.MapFrom(s => Guid.NewGuid()))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.Parameters, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<CriterionVM, CriterionDTO>(model);
                    var update = await criterionService.CreateAsync(modelDto);
                    return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                }
                else
                {
                    var listErrors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
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
                    loggingService.Warn($"{user} Criterion Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Criterion Empty Id");
                }
                var deleted = await criterionService.DeleteAsync(id);
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