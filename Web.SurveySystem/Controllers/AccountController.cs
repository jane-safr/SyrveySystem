using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models;

namespace Web.SurveySystem.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        readonly ILoggerService<AccountController> loggingService;
        readonly IUserService userService;
        readonly IRoleService roleService;

        public AccountController(IUserService serviceUser, IRoleService serviceRole, ILoggerService<AccountController> loggingService)
        {
            this.userService = serviceUser;
            this.roleService = serviceRole;
            this.loggingService = loggingService;
        }
        
        // GET: /Account/Login
        [AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return await Task.Run(()=>View("Login"));
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.LoginName) || string.IsNullOrWhiteSpace(model.Password))
            {
                loggingService.Error("Empty login or password");
                return JsonNetResult.Warn("Empty login or password");
            }
            //Если Email
            if (HelperVm.ValidateMail(model.LoginName))
            {
                model.LoginName = model.LoginName.Substring(0, model.LoginName.IndexOf("@", StringComparison.CurrentCultureIgnoreCase));
            }
            // validate the credentials
            var isAuthenticated = await userService.IsAuthenticated(model.LoginName.Trim(), model.Password.Trim());
            if (isAuthenticated.Succedeed)
            {
                List<string> roles;
                loggingService.Info($"{model.LoginName} - {isAuthenticated.Message}");
                loggingService.Info($"{model.LoginName} - GetUserDataDomain");
                //Получаем данные пользователя
                var userAd = await userService.GetDomainUsersByName(model.LoginName.Trim());
                if (userAd != null)
                {
                    //Проверяем есть ли пользователь в Identity
                    var verify = await userService.VerifyAndCreateIdentity(userAd, model.Password.Trim());
                    if (!verify.Succedeed)
                    {
                        loggingService.Info($"{verify.Message}");
                        return JsonNetResult.Warn($@"{verify.Message}");
                    }
                    //Get Roles User
                    roles = await roleService.GetRoleByUserId(userAd.Id);
                    if (roles == null || !roles.Any())
                    {
                        loggingService.Error("User not in role");
                        ModelState.AddModelError("", @"User not in role");
                        return JsonNetResult.Warn(@"User not in roles");
                    }
                    //Создаем ClaimsIdentity
                    var identity = CreateIdentity(userAd, roles);
                    identity.AddClaim(new Claim("DisplayName", userAd.DisplayName));
                    //Объявляем менеджера
                    IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut("ApplicationCookie");
                    authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
                    FormsAuthentication.SetAuthCookie(userAd.UserName, false);
                    //Проверем был ли указа пользователем адрес редиректа
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return new JsonNetResult(new
                        {
                            success = true,
                            redirectUrl = Url.Action("Index", "Home"),
                            message = "Ok"
                        });
                    }
                    else
                    {
                        loggingService.Info($"Пользователь {userAd.UserName} redirectToAction {returnUrl}");
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return new JsonNetResult(new { success = true, redirectUrl = returnUrl, message = "Ok" });
                        }
                        else
                        {
                            return new JsonNetResult(new
                            {
                                success = false,
                                redirectUrl = Url.Action("Login", "Account"),
                                message = "Link invalid"
                            });
                        }
                    }
                }
            }
            else
            {
                loggingService.Error($"{isAuthenticated.Message}");
                return JsonNetResult.Warn($"{isAuthenticated.Message}");
            }
            return JsonNetResult.Warn(@"Error");
        }

        private ClaimsIdentity CreateIdentity(ApplicationUserDTO userPrincipal, IReadOnlyCollection<string> roles)
        {
            //logger.Info($"StartLogin user: {userPrincipal.SamAccountName}");
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Active Directory"));
            identity.AddClaim(new Claim(ClaimTypes.Name, userPrincipal.SecurityStamp ?? userPrincipal.UserName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userPrincipal.Id));
            //Email
            if (!string.IsNullOrEmpty(userPrincipal.Email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, userPrincipal.Email));
            }
            //Roles
            if (roles.Any())
            {
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.Trim()));
                    loggingService.Info($"{userPrincipal.UserName} in Role: {role}");
                }
            }
            else
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                loggingService.Info($"Login user: {userPrincipal.UserName} Role: user");
            }
            return identity;
        }
        
        
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            loggingService.Info($"LogOff User: {User.Identity.GetUserName()}");
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }
      
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}