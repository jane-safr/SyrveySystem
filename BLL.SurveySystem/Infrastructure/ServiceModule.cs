using Autofac;
using Domain.SurveySystem.Interfaces;
using Domain.SurveySystem.Repository;

namespace BLL.SurveySystem.Infrastructure
{
    public class ServiceModule : Module
    {
        private string connectionString;
        public ServiceModule(string connection)
        {
            if (!string.IsNullOrWhiteSpace(connection))
            {
                connectionString = connection.Trim();
            }
        }
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EFUnitOfWork>().As<IUnitOfWork>().WithParameter("connectionString", connectionString).InstancePerRequest();
            base.Load(builder);
        }
    }
}