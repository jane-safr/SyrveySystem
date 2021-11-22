using Autofac;
using Domain.SurveySystem.Interfaces;
using Domain.SurveySystem.Repository;

namespace BLL.SurveySystem.Infrastructure
{
    public class ServiceModuleConsole : Module
    {
        private readonly string connectionString;
        public ServiceModuleConsole(string connection)
        {
            if (!string.IsNullOrWhiteSpace(connection))
            {
                connectionString = connection.Trim();
            }
        }
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EFUnitOfWork>().As<IUnitOfWork>().WithParameter("connectionString", connectionString);
            base.Load(builder);
        }
    }
}
