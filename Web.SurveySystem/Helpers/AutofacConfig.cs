using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using BLL.SurveySystem.Services;
using Domain.SurveySystem.Context;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using RoleService = BLL.SurveySystem.Services.RoleService;

namespace Web.SurveySystem.Helpers
{
    public class AutofacConfig
    {
        public static IAppBuilder ConfigureContainer(IAppBuilder app)
        {
            if (app == null)
            {
                throw new HttpException(404, "Autofac app Empty");
            }
            // получаем экземпляр контейнера
            var builderConnectionString = new ContainerBuilder();
            builderConnectionString.RegisterType<ConnectionString>().As<IConnectionString>().SingleInstance();
            var containerDb = builderConnectionString.Build();
            var database = containerDb.Resolve<IConnectionString>();

            // получаем экземпляр контейнера
            var builder = new ContainerBuilder();
            builder.RegisterType<SurveySystemContext>().AsSelf().InstancePerRequest();
            builder.RegisterType<UserStore<ApplicationUser>>().As<IUserStore<ApplicationUser>>();
            builder.RegisterType<RoleStore<IdentityRole>>().As<IRoleStore<IdentityRole, string>>();
            builder.RegisterType<ApplicationUserManager>();
            builder.RegisterType<ApplicationRoleManager>();
            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerRequest();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();

            // регистрируем контроллер в текущей сборке
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register dependencies in filter attributes
            builder.RegisterFilterProvider();
            // Register dependencies in custom views
            builder.RegisterSource(new ViewRegistrationSource());

            // 9. Instruction, UploadedFile
            builder.RegisterType<InstructionService>().As<IInstructionService>();
            builder.RegisterType<UploadedFileService>().As<IUploadedFileService>();

            // 8. UserAnswers
            builder.RegisterType<UserAnswerService>().As<IUserAnswerService>();

            //7. Answers + FixedAnswers
            builder.RegisterType<AnswerService>().As<IAnswerService>();
            builder.RegisterType<FixedAnswerService>().As<IFixedAnswerService>();

            //6. Questions + Types
            builder.RegisterType<QuestionTypeService>().As<IQuestionTypeService>();
            builder.RegisterType<QuestionService>().As<IQuestionService>();

            // 5. Notification
            builder.RegisterType<NotificationService>().As<INotificationService>();
            builder.RegisterType<NotificationTypeService>().As<INotificationTypeService>();

            // 4. Invitation
            builder.RegisterType<InvitationService>().As<IInvitationService>();

            // 3. Criterion, Parameter, Indicator
            builder.RegisterType<CriterionService>().As<ICriterionService>();
            builder.RegisterType<ParameterService>().As<IParameterService>();
            builder.RegisterType<IndicatorService>().As<IIndicatorService>();

            // 2. Survey, SurveyType
            builder.RegisterType<SurveyTypeService>().As<ISurveyTypeService>();
            builder.RegisterType<SurveyService>().As<ISurveyService>();
            
            // 1. Settings, Logger
            builder.RegisterType<SettingService>().As<ISettingService>();
            builder.RegisterGeneric(typeof(LoggerService<>)).As(typeof(ILoggerService<>)).InstancePerDependency();
            // 0. Users, Roles
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<RoleService>().As<IRoleService>();

            //Регистрируем ConnectionString
            builder.RegisterType<ConnectionString>().As<IConnectionString>().SingleInstance();
            // Register our Data dependencies
            builder.RegisterModule(new ServiceModule(database.ConString));
            // создаем новый контейнер с теми зависимостями, которые определены выше
            var container = builder.Build();
            // установка сопоставителя зависимостей
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            // REGISTER WITH OWIN
            app.UseAutofacMiddleware(container);
            return app;
        }

        private class ConnectionString : IConnectionString
        {
            private string _connectionStringPrivate;
            private static object lockobj = new object();
            public string ConString
            {
                get
                {
                    if (string.IsNullOrEmpty(_connectionStringPrivate))
                    {
                        lock (lockobj)
                        {
                            if (string.IsNullOrEmpty(_connectionStringPrivate))
                                _connectionStringPrivate = ConfigurationManager.ConnectionStrings["SurveySystemConnection"].ConnectionString;
                        }
                    }
                    return _connectionStringPrivate ?? string.Empty;
                }
            }
        }
        private interface IConnectionString
        {
            string ConString { get; }
        }
    }
}