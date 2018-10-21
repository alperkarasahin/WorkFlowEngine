using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using Hangfire;
using System;
using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._Context;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Services.CustomForms;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web
{

    public class ContainerJobActivator : JobActivator
    {
        private IContainer _container;

        public ContainerJobActivator(IContainer container)
        {
            _container = container;
        }
        public override object ActivateJob(Type type)
        {
            return _container.Resolve(type);
        }
    }

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


            builder.RegisterType<DataContext>()
                .As<IDbContext>()
                .InstancePerBackgroundJob()
                .InstancePerDependency();


            builder.RegisterType<TestWorkFlowForm>()
                .AsSelf()
                .InstancePerBackgroundJob()
                .InstancePerDependency();

            builder.RegisterType<HealthInformationWorkFlowForm>()
                .AsSelf()
                .InstancePerBackgroundJob()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(BaseRepository<>))
                .As(typeof(IRepository<>))
                .InstancePerDependency();



            builder.RegisterType<DecisionMethodService>()
                 .AsSelf()
                 .InstancePerLifetimeScope();

            builder.RegisterType<DocumentService>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterType<FormService>()
                .AsSelf()
                .InstancePerLifetimeScope();



            builder.RegisterType<WorkFlowDataService>()
                .AsSelf()
                .InstancePerBackgroundJob()
                .InstancePerLifetimeScope();


            builder.RegisterType<TestWorkFlowProcessService>()
                .AsSelf()
                .InstancePerLifetimeScope();


            builder.RegisterType<WorkFlowProcessService>()
                .AsSelf()
                .InstancePerLifetimeScope();


            builder.RegisterType<WorkFlowService>()
                .AsSelf()
                .InstancePerLifetimeScope();



            var container = builder.Build();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Process, ProcessVM>()
                    .ForMember(a => a.IsCondition, opt => opt.MapFrom(c => (c.GetType() == typeof(Condition) || c.GetType() == typeof(DecisionPoint))))
                    .ForMember(a => a.ConditionId, opt => opt.MapFrom(c => (c as ConditionOption).ConditionId));


                cfg.CreateMap<WorkFlowTrace, WorkFlowTrace>()
                .ForMember(dest => dest.ConditionOption, opt => opt.Ignore())
                .ForMember(dest => dest.Process, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore());

                cfg.CreateMap<WorkFlowFormViewModel, WorkFlowTrace>();
                cfg.CreateMap<WorkFlowTrace, WorkFlowFormViewModel>();

                cfg.CreateMap<ProcessForm, Process>()
                    .ConstructUsing(x => new Process())
                    .Include<ProcessForm, Condition>()
                    .Include<ProcessForm, SubProcess>()
                    .Include<ProcessForm, ConditionOption>()
                    .Include<ProcessForm, DecisionPoint>()
                    .ForMember(a => a.MonitoringRoleList,
                        opt => opt.MapFrom(c => c.MonitoringRoleList.Where(x => x.IsChecked == true).Select(t => new ProcessMonitoringRole
                        {
                            ProcessId = c.Id,
                            ProjectRole = t.ProjectRole
                        })));

                cfg.CreateMap<ProcessForm, Condition>()
                    .ConstructUsing(x => new Condition());
                cfg.CreateMap<ProcessForm, ConditionOption>()
                    .ConstructUsing(x => new ConditionOption());
                cfg.CreateMap<ProcessForm, DecisionPoint>()
                    .ConstructUsing(x => new DecisionPoint());
                cfg.CreateMap<ProcessForm, SubProcess>()
                    .ConstructUsing(x => new SubProcess());

                cfg.CreateMap<DecisionMethodViewModel, DecisionMethod>();
                cfg.CreateMap<FormViewViewModel, FormView>();
                cfg.CreateMap<Process, Process>();
                cfg.CreateMap<Condition, Condition>();
                cfg.CreateMap<ConditionOption, ConditionOption>();
                cfg.CreateMap<DecisionPoint, DecisionPoint>();

                cfg.CreateMap<ProcessMonitoringRole, MonitoringRoleCheckbox>()
                    .ForMember(a => a.IsChecked, opt => opt.MapFrom(c => true));

                cfg.CreateMap<Process, ProcessForm>()
                    .ForMember(a => a.ConditionId, opt => opt.MapFrom(c => (c as ConditionOption).ConditionId))
                    .ForMember(a => a.ConditionName, opt => opt.MapFrom(c => (c as ConditionOption).Condition.Name))
                    .ForMember(a => a.ProcessType, opt => opt.MapFrom(c => ProcessType.Process))
                    .ForMember(a => a.Value, opt => opt.MapFrom(c => (c as ConditionOption).Value));

                cfg.CreateMap<Condition, ProcessForm>()
                    .ForMember(a => a.ProcessType, opt => opt.MapFrom(c => ProcessType.Condition));

                cfg.CreateMap<SubProcess, ProcessForm>()
                    .ForMember(a => a.ProcessType, opt => opt.MapFrom(c => ProcessType.SubProcess));

                cfg.CreateMap<DecisionPoint, ProcessForm>()
                    .ForMember(a => a.ProcessType, opt => opt.MapFrom(c => ProcessType.DecisionPoint))
                    .ForMember(a => a.DecisionMethodId, opt => opt.MapFrom(c => c.DecisionMethodId))
                    .ForMember(a => a.RepetitionFrequenceByHour, opt => opt.MapFrom(c => c.RepetitionFrequenceByHour));

                cfg.CreateMap<ConditionOption, ProcessForm>()
                    .ForMember(a => a.ProcessType, opt => opt.MapFrom(c => ProcessType.OptionList));

                cfg.CreateMap<TestForm, TestWorkFlowFormViewModel>();
                cfg.CreateMap<TestWorkFlowFormViewModel, TestForm>();

                cfg.CreateMap<WorkFlowFormViewModel, TestWorkFlowFormViewModel>();
                cfg.CreateMap<WorkFlowFormViewModel, SubBusinessProcessViewModel>();

            }
            );


            GlobalConfiguration.Configuration.UseActivator(new ContainerJobActivator(container));
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));



        }
    }
}