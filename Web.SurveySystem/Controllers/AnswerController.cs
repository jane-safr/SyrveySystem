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
    [RoutePrefix("answer")]
    public class AnswerController : Controller
    {
        private readonly IAnswerService answerService;
        private ILoggerService<AnswerController> loggingService;
        public AnswerController(IAnswerService answerService, ILoggerService<AnswerController> loggingService)
        {
            this.answerService = answerService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Answer");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpPost]
        [Route("active")]
        public async Task<JsonNetResult> GetActiveAnswers(List<FilterModels> filterModels)
        {
            try
            {
                var answers = await answerService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<AnswerDTO>, List<AnswerVM>>(answers);
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
        [Route("add")]
        public async Task<ActionResult> Create(AnswerVM model)
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
                    if (model.AnswerId == null || model.AnswerId == Guid.Empty)
                    {
                        var chEmplId = Guid.NewGuid();
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<AnswerVM, AnswerDTO>()
                                .ForMember(x => x.AnswerId, m => m.MapFrom(s => chEmplId))
                                .ForMember(v => v.Question, v => v.Ignore())
                               .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                                .ForMember(x => x.CreatedBy, x => x.MapFrom(m => userName));
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelSave = mapper.Map<AnswerVM, AnswerDTO>(model);
                        var created = await answerService.CreateAsync(modelSave);

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
                            c.CreateMap<AnswerVM, AnswerDTO>()
                                .ForMember(v => v.Question, v => v.Ignore());
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelUp = mapper.Map<AnswerVM, AnswerDTO>(model);
                        var update = await answerService.UpdateAsync(modelUp);
                        return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                    }
                }
                else
                {
                    var listErrors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception ex)
            {
                loggingService.Error($"{ex.Message}");
                return JsonNetResult.Failure(ex.Message);
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
                    loggingService.Warn($"{user} Answer Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Answer Empty Id");
                }
                var deleted = await answerService.DeleteAsync(id);
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