using System;
using Domain.SurveySystem.Entity;
using Microsoft.AspNet.Identity;

namespace Domain.SurveySystem.Identity
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
            UserValidator = new UserValidator<ApplicationUser>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Настройка логики проверки паролей
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = false,
                RequireUppercase = false
            };

            // Настройка параметров блокировки по умолчанию
            UserLockoutEnabledByDefault = false;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //Максимальное количество попыток доступа, разрешенных до блокировки пользователя (если блокировка включена).
            MaxFailedAccessAttemptsBeforeLockout = 5;
        }
    }
}