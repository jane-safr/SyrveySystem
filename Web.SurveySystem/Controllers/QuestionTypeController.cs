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
    [RoutePrefix("questiontype")]
    public class QuestionTypeController : Controller
    {
        private readonly IQuestionTypeService questionTypeService;
        private ILoggerService<QuestionTypeController> loggingService;

        public QuestionTypeController(IQuestionTypeService questionTypeService, ILoggerService<QuestionTypeController> loggingService)
        {
            this.questionTypeService = questionTypeService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> QuestionType");
                return RedirectToAction("Forbidden", "Error");
            }

            return View();
        }

        [HttpGet]
        [Route("all")]
        public async Task<JsonNetResult> GetQuestionTypes()
        {
            try
            {
                var qTypeDto = await questionTypeService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var qTypes = mapper.Map<IEnumerable<QuestionTypeDTO>, List<QuestionTypeVM>>(qTypeDto);
                return new JsonNetResult(new {success = true, data = qTypes });
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
        public async Task<JsonNetResult> GetActiveQuestionTypes(List<FilterModels> filterModels)
        {
            try
            {
                var qTypes = await questionTypeService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<QuestionTypeDTO>, List<QuestionTypeVM>>(qTypes);
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
        public async Task<JsonNetResult> Update(QuestionTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<QuestionTypeVM, QuestionTypeDTO>()
                            .ForMember(s => s.Questions, map => map.Ignore())
                            .ForMember(s => s.FixedAnswers, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<QuestionTypeVM, QuestionTypeDTO>(model);
                    var update = await questionTypeService.UpdateAsync(modelDto);
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
        public async Task<JsonNetResult> Create(QuestionTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<QuestionTypeVM, QuestionTypeDTO>()
                            .ForMember(x => x.QuestionTypeId, m => m.MapFrom(s => Guid.NewGuid()))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.Questions, map => map.Ignore())
                            .ForMember(s => s.FixedAnswers, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<QuestionTypeVM, QuestionTypeDTO>(model);
                    var update = await questionTypeService.CreateAsync(modelDto);
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
                    loggingService.Warn($"{user} QuestionType Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / QuestionType Empty Id");
                }

                var deleted = await questionTypeService.DeleteAsync(id);
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