using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
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
    [RoutePrefix("indicator")]
    public class IndicatorController : Controller
    {
        private readonly IIndicatorService indicatorService;
        private ILoggerService<IndicatorController> loggingService;

        public IndicatorController(IIndicatorService indicatorService, ILoggerService<IndicatorController> loggingService)
        {
            this.indicatorService = indicatorService;
            this.loggingService = loggingService;
        }
        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> Indicator");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Route("all")]
        public async Task<JsonNetResult> GetIndicators()
        {
            try
            {
                var filterModels = new List<FilterModels>
                {
                    new FilterModels
                      {
                          Field = "IsActive",
                          Value = "true"
                      }
                };

                var indicDto = (await indicatorService.FindByFilterAsync(filterModels)).ToList();
                var mapper = MapperConfigVm.MapperConfigAll();
                var indicators = mapper.Map<IEnumerable<IndicatorDTO>, List<IndicatorVM>>(indicDto);
                return new JsonNetResult(new { success = true, data = indicators });
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
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Downstream)]
        [Route("active")]
        public async Task<JsonNetResult> GetActiveIndicators(string searchtxt)
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
                var ind = await indicatorService.FindByFilterAsync(filter);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<IndicatorDTO>, List<IndicatorVM>>(ind);
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
        public async Task<JsonNetResult> Update(IndicatorVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<IndicatorVM, IndicatorDTO>()
                            .ForMember(s => s.Parameter, map => map.Ignore())
                            .ForMember(s => s.Questions, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<IndicatorVM, IndicatorDTO>(model);
                    var update = await indicatorService.UpdateAsync(modelDto);
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
                    loggingService.Warn($"{user} Indicator Empty Id");
                    return JsonNetResult.Failure("Ошибка удаления / Indicator Empty Id");
                }
                var deleted = await indicatorService.DeleteAsync(id);
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
        public async Task<ActionResult> Create(IndicatorVM model)
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
                    if (model.IndicatorId == null || model.IndicatorId == Guid.Empty)
                    {
                        var chEmplId = Guid.NewGuid();
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<IndicatorVM, IndicatorDTO>()
                                .ForMember(x => x.IndicatorId, m => m.MapFrom(s => chEmplId))
                                .ForMember(v => v.Parameter, v => v.Ignore())
                                .ForMember(v => v.Questions, v => v.Ignore())
                                .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                                .ForMember(x => x.CreatedBy, x => x.MapFrom(m => userName));
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelSave = mapper.Map<IndicatorVM, IndicatorDTO>(model);
                        var created = await indicatorService.CreateAsync(modelSave);


                        if (created != null && created.Succedeed)
                        {
                            return new JsonNetResult(new { success = created.Succedeed }); 
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
                            c.CreateMap<IndicatorVM, IndicatorDTO>()
                                .ForMember(x => x.Parameter, x => x.Ignore());
                        });
                        config.AssertConfigurationIsValid();
                        var mapper = config.CreateMapper();
                        var modelUp = mapper.Map<IndicatorVM, IndicatorDTO>(model);
                        var update = await indicatorService.UpdateAsync(modelUp);
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

        [HttpPost]
        [Route("byparameter")]
        public async Task<JsonNetResult> GetIndicators(Guid parameterId)
        {
            try
            {
                if (parameterId == Guid.Empty)
                {
                    return JsonNetResult.Warn("Неверный идентификатор / Invalid identifier");
                }

                var filterModels = new List<FilterModels>
                {
                    new FilterModels
                    {
                        Field = "ParameterId",
                        Value = parameterId.ToString()
                    },
                  /*  new FilterModels
                    {
                        Field = "IsActive",
                        Value = "true"
                    }*/
                };
            
                var resultList = (await indicatorService.FindByFilterAsync(filterModels)).ToList();
                var mapper = MapperConfigVm.MapperConfigAll();
                var indicators = mapper.Map<IEnumerable<IndicatorDTO>, List<IndicatorVM>>(resultList);
               return new JsonNetResult(new { success = true, data = indicators });
            }
            catch (Exception e)
            {
                loggingService.Error($"GetIndicat: {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }
    }
}