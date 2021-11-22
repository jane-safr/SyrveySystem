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
    [RoutePrefix("fixedanswer")]
    public class FixedAnswerController : Controller
    {
        private readonly IFixedAnswerService fixAnswerService;
        private readonly ILoggerService<FixedAnswerController> loggingService;

        public FixedAnswerController(IFixedAnswerService fixAnswerService, ILoggerService<FixedAnswerController> loggingService)
        {
            this.fixAnswerService = fixAnswerService;
            this.loggingService = loggingService;
        }
        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> FixedAnswer");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Route("all")]
        public async Task<JsonNetResult> GetFixedAnswers()
        {
            try
            {
                var fixAnswDto = await fixAnswerService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var fixedAnswers = mapper.Map<IEnumerable<FixedAnswerDTO>, List<FixedAnswerVM>>(fixAnswDto);
                return new JsonNetResult(new { success = true, data = fixedAnswers });
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
        public async Task<JsonNetResult> GetActiveFixedAnswers(List<FilterModels> filterModels)
        {
            try
            {
                var qTypes = await fixAnswerService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<FixedAnswerDTO>, List<FixedAnswerVM>>(qTypes);
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
        public async Task<JsonNetResult> Update(FixedAnswerVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c => { c.CreateMap<FixedAnswerVM, FixedAnswerDTO>().ForMember(v => v.QuestionType, v => v.Ignore()); });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<FixedAnswerVM, FixedAnswerDTO>(model);
                    var update = await fixAnswerService.UpdateAsync(modelDto);
                    return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
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
        [Route("remove")]
        public async Task<JsonNetResult> Delete(Guid id)
        {
            try
            {
                var user = User.Identity.GetUserName();
                if (id == Guid.Empty)
                {
                    loggingService.Warn($"{user} FixedAnswer Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / FixedAnswer Empty Id");
                }
                var deleted = await fixAnswerService.DeleteAsync(id);
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

        [HttpPost]
        [Route("add")]
        public async Task<ActionResult> Create(FixedAnswerVM model)
        {
            try
            {
                if (model == null)
                {
                    return new JsonNetResult(new { success = false, message = "Empty model" });
                }
                var userName = User.Identity.GetUserName();
                if (ModelState.IsValid)
                {
                    //Create
                    if (model.FixedAnswerId == null || model.FixedAnswerId == Guid.Empty)
                    {
                        var chEmplId = Guid.NewGuid();
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<FixedAnswerVM, FixedAnswerDTO>()
                                .ForMember(x => x.FixedAnswerId, m => m.MapFrom(s => chEmplId))
                                .ForMember(v => v.QuestionType, v => v.Ignore())
                               .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                                .ForMember(x => x.CreatedBy, x => x.MapFrom(m => userName));
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelSave = mapper.Map<FixedAnswerVM, FixedAnswerDTO>(model);
                        var created = await fixAnswerService.CreateAsync(modelSave);


                        if (created != null && created.Succedeed)
                        {
                            return new JsonNetResult(new { success = created.Succedeed, message = created.Message });
                        }
                        else
                        {
                            return new JsonNetResult(new
                            {
                                success = false,
                                message = string.IsNullOrEmpty(created?.Message) ? "Ошибка / Еггог" : created.Message
                            });
                        }
                    }
                    //update
                    else
                    {
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<FixedAnswerVM, FixedAnswerDTO>()
                                .ForMember(v => v.QuestionType, v => v.Ignore());
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelUp = mapper.Map<FixedAnswerVM, FixedAnswerDTO>(model);
                        var update = await fixAnswerService.UpdateAsync(modelUp);
                        return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                    }
                }
                else
                {
                    var listErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception ex)
            {
                loggingService.Error($"{ex.Message}");
                return JsonNetResult.Failure(ex.Message);
            }
        }
    }
}