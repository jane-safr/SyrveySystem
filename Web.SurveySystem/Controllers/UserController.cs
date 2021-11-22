using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Interfaces;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("users")]
    public class UserController : Controller
    {
        private readonly IUserService userService;
        private readonly IRoleService roleService;
        private readonly ILoggerService<UserController> loggingService;

        public UserController(ILoggerService<UserController> logServ, IRoleService roleService, IUserService userService)
        {
            this.loggingService = logServ;
            this.userService = userService;
            this.roleService = roleService;
        }
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        // GET: ManagerUser
        [HttpGet]
        [Authorize]
        [Route("index")]
        public ActionResult Index()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("manager"))
            {
                loggingService.Error("Access is denied");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public async Task<ActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                loggingService.Error("User edit guid empty");
                return RedirectToAction("Forbidden", "Error");
            }
            //Если обычный user, берем его Id
            if (!User.IsInRole("admin") && !User.IsInRole("manager"))
            {
                if (!Guid.TryParse(User.Identity.GetUserId(), out id))
                {
                    loggingService.Error("UserId Invalid");
                    return RedirectToAction("Forbidden", "Error");
                }
            }
            var user = await userService.GetUserById(id.ToString());
            if (user == null)
            {
                loggingService.Error("User edit guid empty");
                return RedirectToAction("Forbidden", "Error");
            }
            ViewBag.UserId = id;
            return View("Edit");
        }

        [HttpGet]
        [Authorize]
        [Route("user")]
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Downstream)]
        public async Task<JsonNetResult> GetUserById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("User edit guid empty");
                    return JsonNetResult.Warn("Отсутствует Id / UserId empty");
                }
                if (!User.IsInRole("admin") && !User.IsInRole("manager") && !User.IsInRole("hr"))
                {
                    if (!Guid.TryParse(User.Identity.GetUserId(), out id))
                    {
                        loggingService.Error("User edit guid empty");
                        return JsonNetResult.Warn("Отсутствует Id / UserId empty");
                    }
                }
                var user = await userService.GetUserById(id.ToString());
                if (user == null)
                {
                    loggingService.Error("User not found in DataBase");
                    return JsonNetResult.Warn("Пользователь не найден / User not found");
                }
                var result = new UserVM
                {
                    Id = Guid.TryParse(user.Id, out id) ? id : Guid.Empty,
                    Email = user.Email,
                    Name = user.DisplayName,
                    IsTwoFactor = user.TwoFactorEnabled,
                    IsExternal = user.IsExternal,
                    IsLockoutEnabled = user.LockoutEnabled,
                    Roles = user.Roles != null && user.Roles.Any()
                            ? user.Roles.Select(r => new RoleVM
                            {
                                Id = r.RoleId,
                                Name = string.Empty
                            }).ToList()
                            : null
                };
                return new JsonNetResult(new { success = true, data = result });
            }
            catch (Exception e)
            {
                loggingService.Error($"GetUsers: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Downstream)]
        [Route("find")]
        public async Task<JsonNetResult> GetDomainUsers(string searchtxt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchtxt))
                {
                    var users = (await userService.GetUsers()).Where(y => !string.IsNullOrEmpty(y.UserName)
                                                                                && !string.IsNullOrEmpty(y.Email))
                        .Select(x => new UserVM
                        {
                            Id = Guid.TryParse(x.Id, out var id) ? id : Guid.Empty,
                            Email = x.Email.Trim(),
                            Name = string.Concat(x.DisplayName, " (", x.Email.Substring(x.Email.IndexOf("@", StringComparison.Ordinal) + 1), ")"),
                            IsExternal = x.IsExternal
                        }).ToList();
                    return new JsonNetResult(new { success = true, data = users.DistinctBy(u => u.Email).OrderBy(x => x.Name).ToList() });
                }
                //Guid
                else if (!string.IsNullOrEmpty(searchtxt) && HelperVm.IsGuid(searchtxt))
                {
                    if (!Guid.TryParse(searchtxt, out var id))
                        return JsonNetResult.Warn("Пользователь не найден");
                    var resUser = await userService.GetDomainUserById(id);
                    if (resUser == null)
                        return JsonNetResult.Warn("Пользователь не найден");
                    var us = new List<UserVM>
                    {
                        new UserVM {
                            Id = Guid.TryParse(resUser.Id, out var _id) ? _id : Guid.Empty,
                            Email = string.IsNullOrEmpty(resUser.Email) ? string.Empty : resUser.Email.Trim(),
                            Name = string.Concat(resUser.DisplayName, " (",resUser.Email.Substring(resUser.Email.IndexOf("@", StringComparison.Ordinal) + 1),")"),
                            IsExternal = resUser.IsExternal
                        }
                    };
                    return new JsonNetResult(new { success = true, data = us });
                }
                //Text
                else if (!string.IsNullOrEmpty(searchtxt) && ! HelperVm.IsGuid(searchtxt))
                {
                    var resUser = (await userService.GetDomainUsersByNameList(searchtxt.Trim()))
                        .Select(x => new UserVM
                        {
                            Id = Guid.TryParse(x.Id, out var id) ? id : Guid.Empty,
                            Email = x.Email.Trim(),
                            IsExternal = x.IsExternal,
                            Name = string.Concat(x.DisplayName, " (", x.Email.Substring(x.Email.IndexOf("@", StringComparison.Ordinal) + 1), ")")
                        }).ToList();
                    return new JsonNetResult(new { success = true, data = resUser.DistinctBy(u => u.Email).OrderBy(x => x.Name).ToList() });
                }
                return JsonNetResult.Warn("Уточните параметры поиска / Refine your search");
            }
            catch (Exception e)
            {
                loggingService.Error($"GetUserToRole: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("save")]
        public async Task<JsonNetResult> UpdateAsync(UserVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(model.Id.ToString()) || !HelperVm.IsGuid(model.Id.ToString()))
                    {
                        return JsonNetResult.Warn("Идентификатор сотрудника недействителен / Employee ID is not correct.");
                    }

                    if (User.IsInRole("admin") || User.IsInRole("manager"))
                    {
                        if (model.Roles == null || !model.Roles.Any())
                        {
                            return JsonNetResult.Warn("Выберите роль / Select role");
                        }
                    }

                    var userModel = new ApplicationUserDTO
                    {
                        DisplayName = model.Name,
                        Id = model.Id.ToString(),
                        Email = model.Email
                    };
                    var update = await userService.UpdateUserAsync(userModel);
                    if (update.Succedeed)
                    {
                        //Обновляем роли
                        if (model.Roles != null && model.Roles.Any())
                        {
                            //Удаляем все роли у пользователя
                            var deleteRoles = await roleService.DeleteUserAllRolesAsync(model.Id.ToString());
                            if (!deleteRoles.Succedeed)
                            {
                                return JsonNetResult.Warn(deleteRoles.Message);
                            }
                            //Добавляем вновь
                            foreach (var role in model.Roles)
                            {
                                var addRole = await roleService.AddUserToRoleAsync(model.Id.ToString(), role.Id);
                                if (!addRole.Succedeed)
                                {
                                    return JsonNetResult.Warn(addRole.Message);
                                }
                            }
                        }
                        return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                    }
                    else
                    {
                        return JsonNetResult.Warn(update.Message);
                    }
                }
                else
                {
                    var listErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error(listErrors);
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"Update SampleReportScheduler: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin,manager")]
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Downstream)]
        [Route("all")]
        public async Task<JsonNetResult> GetAplicationUsers()
        {
            try
            {
                var users = (await userService.GetUsers()).Where(x => !string.IsNullOrEmpty(x?.Id))
                    .Select(u =>
                        new
                        {
                            Id = Guid.TryParse(u.Id, out var id) ? id : Guid.Empty,
                            LockoutEnabled = u.LockoutEnabled,
                            Email = u.Email.Trim(),
                            Name = u.DisplayName + " (" + u.Email.Substring(u.Email.IndexOf("@", StringComparison.Ordinal) + 1) + ")"
                        }).ToList();
                return new JsonNetResult(new { success = true, data = users.DistinctBy(u => u.Email).OrderBy(x => x.Name).ToList() });
            }
            catch (Exception e)
            {
                loggingService.Error($"GetUserToRole: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }
    }
}