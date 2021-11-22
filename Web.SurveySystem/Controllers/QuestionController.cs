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
   [RoutePrefix("question")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService questionService;
        private ILoggerService<QuestionController> loggingService;
        public QuestionController(IQuestionService questionService, ILoggerService<QuestionController> loggingService)
        {
            this.questionService = questionService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Question");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<JsonNetResult> GetQuestions()
        {
            try
            {
                var questDto = await questionService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var quest = mapper.Map<IEnumerable<QuestionDTO>, List<QuestionVM>>(questDto);
                return new JsonNetResult(new { success = true, data = quest });
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
        [Route("active")]
        public async Task<JsonNetResult> GetActiveQuestions(List<FilterModels> filterModels)
        {
            try
            {
                var quest = await questionService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<QuestionDTO>, List<QuestionVM>>(quest);
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
        public async Task<JsonNetResult> Update(QuestionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<QuestionVM, QuestionDTO>()
                            .ForMember(s => s.Answers, map => map.Ignore())
                            .ForMember(s => s.Indicator, map => map.Ignore())
                            .ForMember(s => s.Survey, map => map.Ignore())
                            .ForMember(s => s.QuestionType, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<QuestionVM, QuestionDTO>(model);
                    var update = await questionService.UpdateAsync(modelDto);
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
        public async Task<JsonNetResult> Create(QuestionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<QuestionVM, QuestionDTO>()
                            .ForMember(x => x.QuestionId, m => m.MapFrom(s => Guid.NewGuid()))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.Answers, map => map.Ignore())
                            .ForMember(s => s.Indicator, map => map.Ignore())
                            .ForMember(s => s.Survey, map => map.Ignore())
                            .ForMember(s => s.QuestionType, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<QuestionVM, QuestionDTO>(model);
                    var update = await questionService.CreateAsync(modelDto);
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
                    loggingService.Warn($"{user} Question Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Question Empty Id");
                }
                var deleted = await questionService.DeleteAsync(id);
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

        [HttpGet]
        [Authorize]
        [Route("bytest")]
        public async Task<JsonNetResult> GetAllBySurvey(int codeSurvey)
        {
            try
            {
                if (codeSurvey < 1)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }
                var questions = await questionService.GetBySurveyCodeAsync(codeSurvey);
                var mapper = MapperConfigVm.MapperConfigAll();
                var res = mapper.Map<IEnumerable<QuestionDTO>, List<QuestionVM>>(questions);
                return new JsonNetResult(new { success = true, data = res });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize]
        [Route("bygroup")]
        public async Task<JsonNetResult> GetBySurveyGroup(int group, int codeSurvey)
        {
            try
            {
                if (codeSurvey < 1)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }
                if (group < 1)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid group");
                }
                var filterModels = new List<FilterModels>
                {
                    new FilterModels
                    {
                        Field = "SurveyCode",
                        Value = codeSurvey.ToString()
                    },
                    new FilterModels
                    {
                        Field = "Group",
                        Value = group.ToString()
                    },
                };

                var questions = await questionService.FindByFilterAsync(filterModels);

                var mapper = MapperConfigVm.MapperConfigAll();
                var res = mapper.Map<IEnumerable<QuestionDTO>, List<QuestionVM>>(questions);
                return new JsonNetResult(new { success = true, data = res });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

      [HttpGet]
      [Route("question")]
        public async Task<JsonNetResult> GetQuestionAsync(Guid questionId)
        {
            try
            {
                if (questionId == Guid.Empty)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid id");
                }
            
                var question = await questionService.GetByIdAsync(questionId);

                if (question.QuestionType.IsFixedAnswer)
                {
                    var filterModels = new List<FilterModels>
                    {
                        new FilterModels
                        {
                            Field = "Group",
                            Value = question.Group.ToString()
                        },
                        new FilterModels
                        {
                            Field = "SurveyCode",
                            Value = question.Survey.SurveyCode.ToString()
                        }
                    };
                    var questions = (await questionService.FindByFilterAsync(filterModels));
                    
                    // TODO  НЕ СОРТИРУЕТ ПО КРЕДИТУ ANSWERS, нужен explicit Load() 
                    var mapperMany = MapperConfigVm.MapperConfigAll();
                    var resMany = mapperMany.Map<IEnumerable<QuestionDTO>, List<QuestionVM>>(questions);
                    
                    return new JsonNetResult(new { success = true, data = resMany.OrderBy(x => x.Indicator.FullNumber) });  //OrderBy(x=>x.Indicator.FullNumber) });
                }

                var mapper = MapperConfigVm.MapperConfigAll();
                var res = mapper.Map<QuestionDTO, QuestionVM>(question);
                return new JsonNetResult(new { success = true, data = res });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }
    }
}