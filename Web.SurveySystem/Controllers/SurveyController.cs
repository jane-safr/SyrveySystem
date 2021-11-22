using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using IronBarCode;
using Microsoft.AspNet.Identity;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
   
    [RoutePrefix("survey")]
    public class SurveyController : Controller
    {
        private readonly ISurveyService surveyService;
        private readonly IInvitationService invitationService;
        private ILoggerService<SurveyController> loggingService;
        public SurveyController(ISurveyService surveyService, IInvitationService invitationService, ILoggerService<SurveyController> loggingService)
        {
            this.surveyService = surveyService;
            this.invitationService = invitationService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Authorize]
        [Route("list")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Survey");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<JsonNetResult> GetSurveys()
        {
            try
            {
                var surveysDto = await surveyService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var surveys = mapper.Map<IEnumerable<SurveyDTO>, List<SurveyVM>>(surveysDto);
                return new JsonNetResult(new { success = true, data = surveys });
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
        public async Task<JsonNetResult> GetActiveSurveys(List<FilterModels> filterModels)
        {
            try
            {
                var surveys = await surveyService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<SurveyDTO>, List<SurveyVM>>(surveys);
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
        public async Task<JsonNetResult> Update(SurveyVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<SurveyVM, SurveyDTO>()
                            .ForMember(s => s.SurveyType, map => map.Ignore())
                            .ForMember(s => s.Invitations, map => map.Ignore())
                            .ForMember(s => s.Questions, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<SurveyVM, SurveyDTO>(model);
                    var update = await surveyService.UpdateAsync(modelDto);
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
                loggingService.Error($"Update Survey: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("edit/{id}/{tabId?}")]
        public async Task<ActionResult> SurveyByCode(int id, int? tabId = 0)
        {
            if (id <= 0)
                return RedirectToAction("NotFound", "Error");

            var survey = await surveyService.GetByCodeAsync(id);
            if (survey != null)
            {
                ViewBag.SurveyId = survey.SurveyId;
                ViewBag.SurveyCode = survey.SurveyCode;
                ViewBag.CurrentUserId = this.User.Identity.GetUserId();
                ViewBag.TabId = tabId > 0 ? tabId : 0;
                ViewBag.IsActive = survey.IsActive && survey.Invitations.All(x=>!x.IsAccepted);
                if (survey.IsAnonymous && !string.IsNullOrEmpty(survey.ShortLink))
                {
                    var genBarCode = QRCodeWriter.CreateQrCode(survey.ShortLink, 100, QRCodeWriter.QrErrorCorrectionLevel.Medium);
                    var imageDataUrl = genBarCode.ToDataUrl();
                    if (!string.IsNullOrWhiteSpace(imageDataUrl))
                    {
                        ViewBag.ImageData = imageDataUrl;
                    }
                }
            }
            else
            {
                return RedirectToAction("NotFound", "Error");
            }
            return View("Details");
        }
        
        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("add")]
        public async Task<JsonNetResult> Create(SurveyVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var surveyId = Guid.NewGuid();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<SurveyVM, SurveyDTO>()
                            .ForMember(x => x.SurveyId, m => m.MapFrom(s => surveyId))
                            .ForMember(x => x.SurveyCode, m => m.MapFrom(s => Math.Abs(surveyId.GetHashCode())))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.SurveyType, map => map.Ignore())
                            .ForMember(s => s.Questions, map => map.Ignore())
                            .ForMember(s => s.Invitations, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<SurveyVM, SurveyDTO>(model);
                    var update = await surveyService.CreateAsync(modelDto);
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
                    loggingService.Warn($"{user} Survey Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Survey Empty Id");
                }

                var deleted = await surveyService.DeleteAsync(id);
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
        [Route("bycode")]
        public async Task<JsonNetResult> GetByCode(int codeId)
        {
            try
            {
                if (codeId <= 1)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }
                var survey = await surveyService.GetByCodeAsync(codeId);
                var mapper = MapperConfigVm.MapperConfigAll();
                var surveyVm = mapper.Map<SurveyDTO, SurveyVM>(survey);

               return new JsonNetResult(new { success = true, data = surveyVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"ByCode: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }


        [HttpGet]
        [Route("start/{id}")]
        public async Task<ActionResult> AnonymousStart(int id)
        {
            if (id <= 0)
                return RedirectToAction("NotFound", "Error");

            var survey = await surveyService.GetByCodeAsync(id);
            if (survey != null)
            {
                ViewBag.SurveyId = survey.SurveyId;
                ViewBag.SurveyCode = survey.SurveyCode;
                ViewBag.IsActive = survey.IsActive && survey.Invitations.All(x => !x.IsAccepted);
                if (survey.IsAnonymous && !string.IsNullOrEmpty(survey.ShortLink))
                {
                    // создаем приглашение
                    var newInvit = await invitationService.CreateAnonAsync(id);
                    if (newInvit.Succedeed)
                    {
                        if (!int.TryParse(newInvit.Property, out int codeInvit))
                            return RedirectToAction("NotFound", "Error");

                        return RedirectToAction("StartByCode", "Invitation", new {codeId = codeInvit});
                    }

                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    return RedirectToAction("Forbidden", "Error");
                }
            }

            return RedirectToAction("NotFound", "Error");
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("link")]
        public async Task<JsonNetResult> CreateLinkAsync(int surveyCode)
        {
            try
            {
                if (surveyCode <= 1)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }

                var link = await surveyService.GenerateLinkAsync(surveyCode);
                if(link.Succedeed)
                    return new JsonNetResult(new { success = link.Succedeed, message = link.Message });
                
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
    }
}