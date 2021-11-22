using System.Configuration;
using Autofac;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using BLL.SurveySystem.Services;

namespace Console.SurveySystem.Infrastructure
{
   public class ContainerConfig
    {
        public static IContainer Configure()
        {
            // Container: ConnectionString
            var builderConnectionString = new ContainerBuilder();
            builderConnectionString.RegisterType<ConnectionString>().As<IConnectionString>().SingleInstance();
            var containerDb = builderConnectionString.Build();
            var database = containerDb.Resolve<IConnectionString>();

            var builder = new ContainerBuilder();

            // 2 EmailSend
            builder.RegisterType<EmailSendService>().As<IEmailSendService>();

            // 1 Setting, Logger
            builder.RegisterType<SettingService>().As<ISettingService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterGeneric(typeof(LoggerService<>)).As(typeof(ILoggerService<>)).InstancePerDependency();

            // ConnectionString
            builder.RegisterType<ConnectionString>().As<IConnectionString>().SingleInstance();
            builder.RegisterModule(new ServiceModuleConsole(database.ConString));
            return builder.Build();
        }

        public interface IConnectionString
        {
            string ConString { get; }
        }

        public class ConnectionString : IConnectionString
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
    }
}