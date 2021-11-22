using System;
using Autofac;
using BLL.SurveySystem.Interfaces;
using Console.SurveySystem.Infrastructure;
using NLog;

namespace Console.SurveySystem
{
    class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Logger.Info("Process Start");
#if DEBUG
            args = new[] {"SS_ScheduleSendEmail"};
#endif
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length == 0)
            {
                Logger.Error("Value cannot be an empty collection.");
                throw new ArgumentException("Value cannot be an empty collection.", nameof(args));
            }

            var container = ContainerConfig.Configure();
            using (var scope = container.BeginLifetimeScope())
            {
                try
                {
                    // отправка уведомлений
                    if (!string.IsNullOrWhiteSpace(args[0]) && args[0].Trim().ToLower().StartsWith("SS_ScheduleSendEmail", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Info("ScheduleSendEmail-Start");
                        var scheduleEmailSend = scope.Resolve<IEmailSendService>();
                        var resultScheduleEmailSend = scheduleEmailSend.SendEmailsAsync().GetAwaiter().GetResult();
                        Logger.Info(resultScheduleEmailSend.Message);
                    }
                    else
                    {
                        Logger.Error("Args empty or null");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                Logger.Info("Process completed.");
            }
        }
    }
}