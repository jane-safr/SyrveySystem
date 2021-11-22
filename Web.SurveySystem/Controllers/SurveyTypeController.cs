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
    [RoutePrefix("surveytype")]
    public class SurveyTypeController : Controller
    {
        private readonly ISurveyTypeService surveyTypeService;
        private ILoggerService<SurveyTypeController> loggingService;
        public SurveyTypeController(ISurveyTypeService surveyTypeService, ILoggerService<SurveyTypeController> loggingService)
        {
            this.surveyTypeService = surveyTypeService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> SurveyType");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Route("all")]
        public async Task<JsonNetResult> GetSurveyTypes()
        {
            try
            {
                var typesDto = await surveyTypeService.GetAllAsync();
                var mapper = MapperConfigVm.MapperConfigAll();
                var types = mapper.Map<IEnumerable<SurveyTypeDTO>, List<SurveyTypeVM>>(typesDto);
                return new JsonNetResult(new { success = true, data = types });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetSTypes User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpGet]
        [Route("active")]
        public async Task<JsonNetResult> GetActiveSurveyTypes(string searchtxt)
        {
            try
            {
                var filter = new List<FilterModels>
                {
                    new FilterModels
                    {
                        Field = "IsActive",
                        Value = "true"
                    }
                };
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
                            Field = "searchtxt",
                            Value = searchtxt.Trim()
                        });
                    }
                }
                var types = await surveyTypeService.FindByFilterAsync(filter);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<SurveyTypeDTO>, List<SurveyTypeVM>>(types);
                return new JsonNetResult(new { success = true, data = resVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetActiveSTypes: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("edit")]
        public async Task<JsonNetResult> Update(SurveyTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<SurveyTypeVM, SurveyTypeDTO>()
                            .ForMember(s => s.Surveys, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<SurveyTypeVM, SurveyTypeDTO>(model);
                    var update = await surveyTypeService.UpdateAsync(modelDto);
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
                loggingService.Error($"Update types: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        [Route("add")]
        public async Task<JsonNetResult> Create(SurveyTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<SurveyTypeVM, SurveyTypeDTO>()
                            .ForMember(x => x.SurveyTypeId, m => m.MapFrom(s => Guid.NewGuid()))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(s => s.Surveys, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<SurveyTypeVM, SurveyTypeDTO>(model);
                    var update = await surveyTypeService.CreateAsync(modelDto);
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
                loggingService.Error($"Create SurveyType: {e.Message}");
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
                    loggingService.Warn($"{user} Surveytypes Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления типа / Delete not completed");
                }

                var deleted = await surveyTypeService.DeleteAsync(id);
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
                loggingService.Error($"Delete Types: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }
    }
}