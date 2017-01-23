using Autofac;
using Autofac.Integration.Mvc;
using System.Configuration;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._Context;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web
{


    public class DependencyRegistrar
    {
        public static void RegisterDependencies()
        {


            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterModule(new AutofacWebTypesModule());

            builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

            builder.Register<IDbContext>(c =>
                    new DataContext(ConfigurationManager.ConnectionStrings["WorkFlowManagerDB"].ConnectionString))
                    .InstancePerDependency();



            builder.RegisterType<WorkFlowService>().AsSelf().UsingConstructor(typeof(IUnitOfWork)).InstancePerLifetimeScope();



            builder.RegisterType<DocumentService>().AsSelf().UsingConstructor(typeof(IUnitOfWork)).InstancePerLifetimeScope();



            builder.RegisterType<FormService>().AsSelf().UsingConstructor(typeof(IUnitOfWork)).InstancePerLifetimeScope();
            builder.RegisterType<DecisionMethodService>().AsSelf().UsingConstructor(typeof(IUnitOfWork)).InstancePerLifetimeScope();




            var container = builder.Build();


            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));



        }
    }
}