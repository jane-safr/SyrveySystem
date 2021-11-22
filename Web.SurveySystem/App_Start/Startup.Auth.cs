using System;
using Domain.SurveySystem.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace Web.SurveySystem
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            app.Properties["Microsoft.Owin.Security.Constants.DefaultSignInAsAuthenticationType"] = "ExternalCookie";
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ExternalCookie",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<Domain.SurveySystem.Identity.ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(15),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                },
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(15) //Set the session timeout at here
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
        }
    }
}