using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Common.Mapping;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.DataAccess._Context
{
    public class DataContext : IdentityDbContext, IDbContext
    {
        public DataContext() : base("WorkFlowManagerDB")
        {
            Database.CommandTimeout = 180;
            //Lazy loading
            Configuration.LazyLoadingEnabled = false;
#if DEBUG
            Database.Log = message => Trace.WriteLine(message);
#endif
        }

        public IDbSet<BaseTable> BaseTableTbl { get; set; }

        public IDbSet<WorkFlow> WorkFlowTbl { get; set; }
        public IDbSet<Task> TaskTbl { get; set; }
        public IDbSet<Tables.Process> ProcessTbl { get; set; }
        public IDbSet<ProcessMonitoringRole> ProcessMonitoringRoleTbl { get; set; }
        public IDbSet<Condition> ConditionTbl { get; set; }
        public IDbSet<ConditionOption> ConditionOptionTbl { get; set; }

        public IDbSet<DecisionPoint> DecisionPointTbl { get; set; }
        public IDbSet<DecisionMethod> DecisionMethodTbl { get; set; }

        public IDbSet<FormView> FormViewTbl { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);




            modelBuilder.Entity<MasterTest>().ToTable("MasterTestTbl");
            modelBuilder.Entity<WorkFlowEngineVariable>().ToTable("WorkFlowEngineVariableTbl");

            modelBuilder.Configurations.Add(new BaseTableTableMap());
            modelBuilder.Configurations.Add(new ConditionMap());

            modelBuilder.Configurations.Add(new DocumentMap());

            modelBuilder.Configurations.Add(new TaskMap());
            modelBuilder.Configurations.Add(new ProcessMap());
            modelBuilder.Configurations.Add(new ConditionOptionMap());
            modelBuilder.Configurations.Add(new ProcessMonitoringRoleMap());
            modelBuilder.Configurations.Add(new WorkFlowMap());

            modelBuilder.Configurations.Add(new FormViewMap());
            modelBuilder.Configurations.Add(new DecisionMethodMap());
            modelBuilder.Configurations.Add(new DecisionPointMap());
            modelBuilder.Configurations.Add(new WorkFlowTraceMap());
            modelBuilder.Configurations.Add(new TestFormMap());



            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

        }


        public override int SaveChanges()
        {
            try
            {

                var models = ChangeTracker.Entries()
                .Where(x => x.Entity is BaseTable
                && x.State == EntityState.Added || x.State == EntityState.Modified);

                foreach (var model in models)
                {
                    BaseTable table = model.Entity as BaseTable;
                    if (table != null)
                    {

                        DateTime now = DateTime.Now;

                        if (model.State == EntityState.Added)
                        {
                            table.CreatedTime = now;
                        }
                        else if (model.State == EntityState.Modified)
                        {
                            base.Entry(table).Property(x => x.CreatedTime).IsModified = false;

                            table.UpdatedTime = now;
                        }

                    }
                }
                return base.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();
                foreach (var eve in ex.EntityValidationErrors)
                {
                    string error =
                        string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);


                    foreach (var ve in eve.ValidationErrors)
                    {
                        string local = string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);

                        sb.Append(local.ToString());
                    }
                }
                throw new DbEntityValidationException(sb.ToString(), ex.EntityValidationErrors);
            }
        }


        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

    }
}

