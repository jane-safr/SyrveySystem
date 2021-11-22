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
using Web.SurveySystem.Models;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("invitation")]
    public class InvitationController : Controller
    {
        private readonly IInvitationService invitService;
        private readonly IUserAnswerService userAnswerService;
        private readonly ISurveyService surveyService;

        private ILoggerService<InvitationController> loggingService;
        public InvitationController(IInvitationService invitService, IUserAnswerService usAnswerServ, ISurveyService surveyService,  ILoggerService<InvitationController> loggingService)
        {
            this.invitService = invitService;
            this.userAnswerService = usAnswerServ;
            this.surveyService = surveyService;
            this.loggingService = loggingService;
        }
        [HttpGet]
        [Route("index")]
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<JsonNetResult> GetInvitations()
        {
            try
            {
                var invitationDto = await invitService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var invit = mapper.Map<IEnumerable<InvitationDTO>, List<InvitationVM>>(invitationDto);
                return new JsonNetResult(new { success = true, data = invit });
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
        [Route("invitefilter")]
        public async Task<JsonNetResult> GetAllBtByFilter(List<FilterModels> filterModels)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (filterModels != null && filterModels.Any())
                {
                    if (!filterModels.Any(x => x.Field.StartsWith("dateStart", StringComparison.OrdinalIgnoreCase))
                        && !filterModels.Any(x => x.Field.StartsWith("dateEnd", StringComparison.OrdinalIgnoreCase))
                        && !filterModels.Any(x => x.Field.StartsWith("searchtxt", StringComparison.OrdinalIgnoreCase)))
                    {
                        filterModels.Add(new FilterModels
                        {
                            Field = "dateStart",
                            Value = DateTime.Now.AddMonths(-3).Date.Add(new TimeSpan(0, 0, 00)).ToString("dd.MM.yyyy HH:mm") // за последние 3 месяца
                        });
                        filterModels.Add(new FilterModels
                        {
                            Field = "dateEnd",
                            Value = DateTime.Now.Date.Add(new TimeSpan(23, 59, 00)).ToString("dd.MM.yyyy HH:mm")
                        });
                    }
                }
                else if (this.User.IsInRole("user"))
                {
                    if (filterModels != null && filterModels.Any())
                    {
                        filterModels.Add(new FilterModels
                        {
                            Field = "employeeId",
                            Value = userId
                        });
                    }
                    else
                    {
                        filterModels = new List<FilterModels>
                        {
                            new FilterModels
                            {
                                Field = "employeeId",
                                Value = userId
                            }
                        };
                    }
                }
               
                else
                { // manager, admin
                    filterModels = new List<FilterModels>
                    {
                        new FilterModels
                        {
                            Field = "IsActive",
                            Value = "true"
                        },
                        new FilterModels
                        {
                            Field = "employeeId",
                            Value = userId
                        },
                        new FilterModels
                        {
                            Field = "IsAnonSurvey",
                            Value = "false"
                        }
                    };
                }

                var resultList = (await invitService.FindByFilterAsync(filterModels)).Select(invite => new
                {
                    InvitationId = invite.InvitationId,
                    InvitationCode = invite.InvitationCode,
                    UserId = invite.UserId,
                    UserName = invite.UserName,
                    UserEmail = invite.UserEmail,
                    DateEnd = invite.DateEnd,
                    DateStart = invite.DateStart,
                    CreatedOn = invite.CreatedOn,
                    CreatedBy = invite.CreatedBy,
                    SurveyCode = invite.Survey.SurveyCode,
                    FullName = String.Concat(invite.Survey.NameRus, " / ", invite.Survey.NameEng),
                    Percent = invite.Percent,
                    IsActive = invite.IsActive,
                    IsFinished = invite.IsFinished,
                    IsAccepted = invite.IsAccepted,
                    ActualCompleteDate = invite.ActualCompleteDate
                }).ToList();

                return new JsonNetResult(new { success = true, data = resultList });
            }
            catch (Exception e)
            {
                loggingService.Error($"Filter: {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("start/{codeId}")]
        public async Task<ActionResult> StartByCode(Guid codeId)
        {
            if (codeId == Guid.Empty)
            {
                loggingService.Error($"codeId < 1");
                return await Task.Run(()=> RedirectToAction("NotFound", "Error"));
            }
            var invit = await invitService.GetByIdAsync(codeId);
            if (invit != null)
            {
                if (invit.Survey == null)
                {
                    loggingService.Error($"Survey not found for invitation {invit.InvitationId}");
                    return RedirectToAction("NotFound", "Error");
                }
                if (!invit.Survey.IsAnonymous) // проверяем пользователя и анонимность
                {
                    var userId = User.Identity.GetUserId();
                    if (invit.UserId != new Guid(userId))
                    {
                        return RedirectToAction("Forbidden", "Error");
                    }
                }
                if (invit.IsFinished) // тест уже был завершен
                {
                    loggingService.Error($"test is finished {invit.InvitationId}");
                    return RedirectToAction("TestFinished", "Error");
                }
                if (invit.DateEnd < DateTime.Now) // тест просрочен
                {
                    loggingService.Error("test overdue");
                    return RedirectToAction("TestOverdue", "Error"); 
                }
                if (invit.IsAccepted) // если уже начинали тестирование -> на посл.вопрос без ответа
                {
                    return RedirectToAction("Index", "UserAnswer", new { invitationId = codeId });
                }
                ViewBag.InvitationId = invit.InvitationId;
                ViewBag.InvitationCode = invit.InvitationCode;
                ViewBag.SurveyCode = invit.Survey.SurveyCode;
               // ViewBag.CurrentUserId = user;
                return await Task.Run(()=>View("Start")); // переходим на стартовую страницу теста
            }
            return RedirectToAction("NotFound", "Error"); // если не найдено приглашение в БД
        }

        [HttpGet]
        [Route("finish/{codeId}")]
        public async Task<ActionResult> FinishByCode(int codeId)
        {
            if (codeId < 1)
            {
                loggingService.Error($"codeId < 1 {codeId < 1}");
                return RedirectToAction("NotFound", "Error");
            }

            var invit = await invitService.GetByCodeAsync(codeId);
            if (invit != null)
            {
                if (invit.IsFinished)
                {
                    loggingService.Warn($"{invit.InvitationId} test was already finished");
                    return RedirectToAction("NotFound", "Error");
                }

                if (!invit.Survey.IsAnonymous)
                {
                    //if (invit.UserId != new Guid(User.Identity.GetUserId()))
                    //{
                    //    return RedirectToAction("Forbidden", "Error");
                    //}
                    return RedirectToAction("Forbidden", "Error");
                }
          
                foreach (var answer in invit.UserAnswers) // если не на все вопросы даны ответы
                {
                    if (answer.AnswerId == Guid.Empty || answer.AnswerId == null)
                    {
                        loggingService.Warn($"answer not found for {invit.InvitationId}. test cannot be finished");
                        return RedirectToAction("Index", "UserAnswer", new { invitationId = codeId });
                    }
                }
                //update
                invit.ActualCompleteDate = DateTime.Now;
                invit.IsFinished = true;
                invit.Percent = 100;
                
                var update = await invitService.UpdateAsync(invit);
                if (update.Succedeed)
                {
                    loggingService.Info("Survey completed");
                    ViewBag.SurveyCode = invit.Survey.SurveyCode;
                    return View("Finish");
                }
            }
            return RedirectToAction("NotFound", "Error");
        }

        [HttpGet]
        [Route("initiate/{codeId}")]
        public async Task<ActionResult> Initiate(int codeId)
        {
            try
            {
                if (codeId < 1)
                {
                    loggingService.Error($"codeId < 1 {codeId < 1}");
                    return RedirectToAction("NotFound", "Error");
                }

                var invit = await invitService.GetByCodeAsync(codeId);
                if (invit != null)
                {
                    if (invit.Survey == null)
                    {
                        loggingService.Error($"User {User.Identity.Name} cannot get survey or its questions");
                        return RedirectToAction("NotFound", "Error");
                    }
                    if (!invit.Survey.IsAnonymous && !User.IsInRole("admin") && !User.IsInRole("manager")) // проверяем пользователя и анонимность
                    {
                         var userId = User.Identity.GetUserId();
                         if (invit.UserId != new Guid(userId))
                         {
                             return RedirectToAction("Forbidden", "Error");
                         }
                       // return RedirectToAction("Forbidden", "Error");
                    }
                    
                    //update
                    invit.DateStart = DateTime.Now;
                    invit.IsAccepted = true;
                    invit.Percent = 0;
                   // invit.LastQuestionId = invit.Survey.Questions.OrderBy(o => o.Group).FirstOrDefault()?.QuestionId; //
                    var update = await invitService.UpdateAsync(invit);
                    //TODO
                    if (update.Succedeed)
                    {
                        var usAnswers = await userAnswerService.CreateByInvitationAsync(invit.InvitationId);
                        if (usAnswers.Succedeed)
                        {
                            loggingService.Info($"created Useranswers for {User.Identity.Name} test {invit.Survey.SurveyCode}");
                        }
                        else
                        {
                            loggingService.Error("Useranswers list not created");
                            return RedirectToAction("NotFound", "Error");
                        }
                        return RedirectToAction("Index", "UserAnswer", new {invitationId = codeId });
                    }
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    loggingService.Error("Invitation not found");
                    return RedirectToAction("NotFound", "Error");
                }

            }
            catch (Exception e)
            {
                loggingService.Error($"{e.Message}");
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("invitation")]
        public async Task<JsonNetResult> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return JsonNetResult.Warn("Тест не определен / Test not found");

                var invitation = await invitService.GetByIdAsync(id);
               
                var mapper = MapperConfigVm.MapperConfigAll();

                var res = mapper.Map<InvitationDTO, InvitationVM>(invitation);
                return new JsonNetResult(new {success = true, data = res});
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [Route("bycode")]
        public async Task<JsonNetResult> GetByCode(int codeId)
        {
            try
            {
                if (codeId < 1)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }
                var invit = await invitService.GetByCodeAsync(codeId);
                //if (!invit.Survey.IsAnonymous)
                //{
                //    loggingService.Error("survey is not anon");
                //    return new JsonNetResult(new { success = false });
                //}
                var mapper = MapperConfigVm.MapperConfigAll();
                var invitVm = mapper.Map<InvitationDTO, InvitationVM>(invit);
                return new JsonNetResult(new { success = true, data = invitVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"ByCode: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }


        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("add")]
        public async Task<JsonNetResult> Create(InvitationVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var invitid = Guid.NewGuid();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<InvitationVM, InvitationDTO>()
                            .ForMember(x => x.InvitationId, m => m.MapFrom(s => invitid))
                            .ForMember(x => x.InvitationCode, m => m.MapFrom(s => Math.Abs(invitid.GetHashCode())))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.Survey, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<InvitationVM, InvitationDTO>(model);
                    var update = await invitService.CreateAsync(modelDto);
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
        [Authorize] 
        [Route("addmany")]
        public async Task<JsonNetResult> CreateMultipleAsync(MultipleInvitation model)
        {
            try
            {
                var us = User.Identity.GetUserName();
                if (model.SurveyId == Guid.Empty || !HelperVm.IsGuid(model.SurveyId.ToString()))
                {
                    return JsonNetResult.Warn("Тест не определен / Test not found");
                }
                if (model.DateEnd <= DateTime.Now)
                {
                    return JsonNetResult.Warn("Дата завершения не может быть равна текущей / Date cannot be the same as today");
                }
                if (!model.Users.Any())
                {
                    return JsonNetResult.Warn("Пользователи не найдены / Users not found");
                }

                // проверяем тест, есть ли там вопросы
                var survey = await surveyService.GetByIdAsync(model.SurveyId);
                if (survey == null)
                {
                    loggingService.Error("Test not found");
                    return JsonNetResult.Warn("Тест не определен / Test not found");
                }
                if (!survey.Questions.Any())
                {
                    loggingService.Error("Test not found");
                    return JsonNetResult.Warn("У теста нет вопросов. Невозможно отправить приглашения / Test does not have question. Cannot send invitations");
                }
               // if (!survey.Questions..Contains(x=>x.Answers)
               
                var invitations = new List<InvitationVM>();
                
                foreach (var user in model.Users)
                {
                    Guid invId = Guid.NewGuid();
                    invitations.Add(new InvitationVM
                    {
                        InvitationId = invId,
                        InvitationCode =  Math.Abs(invId.GetHashCode()),
                        CreatedOn = DateTime.Now,
                        CreatedBy = us,
                        SurveyId = model.SurveyId,
                        DateEnd = model.DateEnd,
                        UserId = user.Id,
                        UserName = user.Name.Substring(0, user.Name.LastIndexOf(" ", StringComparison.Ordinal)).Trim(),
                        UserEmail = user.Email,
                        IsActive = true,
                        Percent = 0,
                        IsAccepted = false,
                        IsFinished = false
                    });
                }
                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<InvitationVM, InvitationDTO>()
                        .ForMember(s => s.Survey, map => map.Ignore())
                        .ForMember(s => s.UserAnswers, map => map.Ignore());
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var modelDto = mapper.Map<List<InvitationVM>, List<InvitationDTO>>(invitations);
                
                var add = await invitService.CreateMultipeAsync(modelDto); // 
                if(add.Succedeed)
                return JsonNetResult.SuccessMessage(add.Message);

                return JsonNetResult.Warn(add.Message);
            }

            catch (Exception ex)
            {
                loggingService.Error($"{ex.Message}");

                return JsonNetResult.Failure("Ошибка приглашения / Error invite");
            }
        }

        [HttpPost]
        [Authorize]
        [Route("resendemail")]
        public async Task<JsonNetResult> ResendInvitationAsync(Guid invitationId)
        {
            try
            {
                var user = User.Identity.GetUserName();
                if (invitationId == Guid.Empty)
                {
                    return JsonNetResult.Warn("Невалидный идентификатор / Invalid identifier");
                }
                loggingService.Info($"Пользователь отправил повторное приглашение {user}");
                var resend = await invitService.InvitationResendAsync(invitationId, user);
                return new JsonNetResult(new { success = resend.Succedeed, message = resend.Message });
            }
            catch (Exception ex)
            {
                loggingService.Error(ex);
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
                    loggingService.Warn($"{user} Invitation Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Invitation Empty Id");
                }
                var deleted = await invitService.DeleteAsync(id);
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
                var invitBySurv = await invitService.GetBySurveyCodeAsync(codeSurvey);
                var mapper = MapperConfigVm.MapperConfigAll();

                var res = mapper.Map<IEnumerable<InvitationDTO>, List<InvitationVM>>(invitBySurv);
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