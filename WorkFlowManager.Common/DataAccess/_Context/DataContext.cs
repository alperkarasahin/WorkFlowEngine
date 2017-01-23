using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Common.Mapping;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.DataAccess._Context
{
    public class DataContext : IdentityDbContext, IDbContext
    {
        public DataContext() : base("Server=.; Database=WORKFLOWMANAGER; Integrated Security=sspi;")
        {
            //Lazy loading
            Configuration.LazyLoadingEnabled = false;
        }

        public DataContext(string connectionString) : base(connectionString)
        {
            //Lazy loading
            Configuration.LazyLoadingEnabled = false;
        }

        public IDbSet<BaseTable> BaseTableTbl { get; set; }

        public IDbSet<WorkFlow> WorkFlowTbl { get; set; }
        public IDbSet<Task> TaskTbl { get; set; }
        public IDbSet<Process> ProcessTbl { get; set; }
        public IDbSet<ProcessMonitoringRole> ProcessMonitoringRoleTbl { get; set; }
        public IDbSet<Condition> ConditionTbl { get; set; }
        public IDbSet<ConditionOption> ConditionOptionTbl { get; set; }

        public IDbSet<DecisionPoint> DecisionPointTbl { get; set; }
        public IDbSet<DecisionMethod> DecisionMethodTbl { get; set; }

        public IDbSet<FormView> FormViewTbl { get; set; }





        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<Condition>().ToTable("ConditionTbl");


            modelBuilder.Configurations.Add(new BaseTableTableMap());
            modelBuilder.Configurations.Add(new DocumentMap());

            modelBuilder.Configurations.Add(new TaskMap());
            modelBuilder.Configurations.Add(new ProcessMap());
            modelBuilder.Configurations.Add(new ConditionOptionMap());
            modelBuilder.Configurations.Add(new ProcessMonitoringRoleMap());
            modelBuilder.Configurations.Add(new WorkFlowMap());

            modelBuilder.Configurations.Add(new FormViewMap());
            modelBuilder.Configurations.Add(new DecisionMethodMap());
            modelBuilder.Configurations.Add(new DecisionPointMap());




            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

        }


        public static DataContext Create(string connectionString)
        {
            return new DataContext(connectionString);
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

    }
}
