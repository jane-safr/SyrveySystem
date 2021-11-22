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
    [RoutePrefix("useranswer")]
    public class UserAnswerController : Controller
    {
        private readonly IUserAnswerService userAnswerService;
        private readonly IInvitationService invitationService;
        private ILoggerService<UserAnswerController> loggingService;
        public UserAnswerController(IUserAnswerService userAnswerService, IInvitationService invitService, ILoggerService<UserAnswerController> loggingService)
        {
            this.userAnswerService = userAnswerService;
            this.invitationService = invitService;
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Route("index")]
        public async Task<ActionResult> GetUserQuestionsAsync(int invitationId)
        {
            if (invitationId < 1)
            {
                ViewBag.Error = "Не указан идентификатор ссылки / Link empty.";
                return new JsonNetResult(new { success = false });
            }
            var invitation = await invitationService.GetByCodeAsync(invitationId);
            if (invitation == null)
            {
                return new JsonNetResult(new { success = false });
            }
            if (invitation.IsFinished)
            {
                return View("TestFinished", "Error");
            }

            // получаем только вопросы без ответа
            var filterModels = new List<FilterModels>
            {
                new FilterModels
                {
                    Field = "InvitationId",
                    Value = invitation.InvitationId.ToString()
                },
                new FilterModels
                {
                    Field = "AnswerId",
                    Value = Guid.Empty.ToString()
                }
            };
            var questionsUser = (await userAnswerService.FindByFilterAsync(filterModels)).ToList();
            if (!questionsUser.Any()) // стр завершить
            {
                loggingService.Warn("Questions are empty - finish");
                return RedirectToAction("finish", "Invitation", new {id = invitationId});
            }

            //TODO
            ViewBag.InvitationCode = invitationId;
            ViewBag.InvitationId = invitation.InvitationId;
            ViewBag.QuestionsCount = questionsUser.Count;
            var selQuestion = questionsUser.OrderBy(x => x.Order).FirstOrDefault();
            //  Debug.WriteLine(selQuestion.Invitation.LastQuestionId);

            ViewBag.QuestionId = selQuestion.QuestionId;
            ViewBag.UserAnswerId = selQuestion.UserAnswerId;
            if (questionsUser.Count > 1)
            {
                ViewBag.NextQuestionId = questionsUser.OrderBy(x => x.Order).Skip(1).FirstOrDefault().QuestionId;
            }

            else ViewBag.NextQuestionId = null;

            ViewBag.UserAnswerIds = questionsUser.Select(id => id.UserAnswerId).ToList();
            //window.CurrentUserId = '@(ViewBag.CurrentUserId)';
            return View("Index");
        }


        [HttpGet]
        [Authorize]
        [Route("result/{codeId}")]
        public async Task<ActionResult> GetAnswerResultAsync(int codeId)
        {
            string user = User.Identity.GetUserId();
            if (codeId < 1)
            {
                return View("NotFound", "Error");
            }

            var invit = await invitationService.GetByCodeAsync(codeId);
            if (invit != null)
            {
                if (invit.Survey == null)
                {
                    loggingService.Error($"Survey not found for invitation {invit.InvitationCode}");
                    return RedirectToAction("NotFound", "Error");
                }
                if (invit.UserId != new Guid(user) && !User.IsInRole("admin") && !User.IsInRole("manager")) // только свои результаты
                {
                    return RedirectToAction("Forbidden", "Error");
                }
                if (!invit.IsFinished) // тест еще не завершен
                {
                    loggingService.Error($"test was not finished {invit.InvitationCode}");
                    return RedirectToAction("Forbidden", "Error");
                }

                ViewBag.InvitationId = invit.InvitationId;
                ViewBag.InvitationCode = invit.InvitationCode;
                ViewBag.SurveyCode = invit.Survey.SurveyCode;
                ViewBag.SurveyId = invit.SurveyId;
                ViewBag.CurrentUserId = user;
                return View("Result"); // переходим на страницу результатов теста
            }
            return RedirectToAction("NotFound", "Error"); // если не найдено приглашение в БД
        }

        [Authorize]
        [HttpPost]
        [Route("active")]
        public async Task<JsonNetResult> GetUserAnswers(List<FilterModels> filterModels)
        {
            try
            {
                var answers = await userAnswerService.FindByFilterAsync(filterModels);
                var mapper = MapperConfigVm.MapperConfigAll();
                var resVm = mapper.Map<IEnumerable<UserAnswerDTO>, List<UserAnswerVM>>(answers);
                return new JsonNetResult(new { success = true, data = resVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetAnswers: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
       // [Authorize]
        [Route("addanswer")]
        public async Task<JsonNetResult> Update(UserAnswerVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<UserAnswerVM, UserAnswerDTO>()
                            .ForMember(s => s.Invitation, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<UserAnswerVM, UserAnswerDTO>(model);
                    var update = await userAnswerService.UpdateAsync(modelDto);
                    if (update.Succedeed)
                    {
                        // Обновляем процент в Invitation
                        var invitation = await invitationService.GetByIdAsync(model.InvitationId);
                        if (invitation == null)
                        {
                            return new JsonNetResult(new {success = false});
                        }
                        // кол-во вопросов по InvitationId

                        var filterModels = new List<FilterModels>
                        {
                            new FilterModels
                            {
                                Field = "InvitationId",
                                Value = model.InvitationId.ToString()
                            }
                        };
                        var allQuest = await userAnswerService.FindByFilterAsync(filterModels); // все вопросы
                        if (!allQuest.Any())
                        {
                            loggingService.Warn("No questions - cannot update percent");
                            invitation.Percent = 0;
                        }
                        else
                        {
                            filterModels.Add(new FilterModels
                            {
                                Field = "AnswerId",
                                Value = Guid.Empty.ToString()
                            });
                            var notAnsQuest = await userAnswerService.FindByFilterAsync(filterModels); // без ответа

                            double countPers = (1 - (double)notAnsQuest.Count() / allQuest.Count());
                            invitation.Percent = Convert.ToInt32(countPers * 100);
                        }
                        var upd = await invitationService.UpdateAsync(invitation);
                        if (!upd.Succedeed)
                            loggingService.Warn("Cannot update percent");
                    }

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
       [Route("addmany")]
        public async Task<JsonNetResult> UpdateMany(List<UserAnswerVM> modellist)
        {
            try
            {
             //   var user = User.Identity.GetUserName();
                // if model is empty
                if (!modellist.Any() || modellist.Any(r => !TryValidateModel(r)))
                {
                    var listErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error($"Update List UserAnswers: {listErrors}");
                    return JsonNetResult.Warn(listErrors);
                }

                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<UserAnswerVM, UserAnswerDTO>()
                            .ForMember(s => s.Invitation, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<IEnumerable<UserAnswerVM>, List<UserAnswerDTO>>(modellist);
                    var update = await userAnswerService.UpdateFixedAsync(modelDto);
                    if (update.Succedeed)
                    {
                        // Обновляем процент в Invitation
                        var invitation = await invitationService.GetByIdAsync(modellist.FirstOrDefault().InvitationId);
                        if (invitation == null)
                        {
                            return new JsonNetResult(new {success = false});
                        }
                        // кол-во вопросов по InvitationId

                        var filterModels = new List<FilterModels>
                        {
                            new FilterModels
                            {
                                Field = "InvitationId",
                                Value = modellist.FirstOrDefault().InvitationId.ToString()
                            }
                        };
                        var allQuest = await userAnswerService.FindByFilterAsync(filterModels); // все вопросы
                        if (!allQuest.Any())
                        {
                            loggingService.Warn("No questions - cannot update percent");
                            invitation.Percent = 0;
                        }
                        else
                        {
                            filterModels.Add(new FilterModels
                            {
                                Field = "AnswerId",
                                Value = Guid.Empty.ToString()
                            });
                            var notAnsQuest = await userAnswerService.FindByFilterAsync(filterModels); // без ответа

                            double countPers = (1 - (double)notAnsQuest.Count() / allQuest.Count());

                            invitation.Percent = Convert.ToInt32(countPers * 100);
                        }

                        var upd = await invitationService.UpdateAsync(invitation);
                        if (!upd.Succedeed)
                            loggingService.Warn("Cannot update percent");
                    }
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
    }
}