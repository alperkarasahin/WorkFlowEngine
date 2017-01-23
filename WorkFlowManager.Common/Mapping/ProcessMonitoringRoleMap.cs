using System.Data.Entity.ModelConfiguration;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class ProcessMonitoringRoleMap : EntityTypeConfiguration<ProcessMonitoringRole>
    {
        public ProcessMonitoringRoleMap()
        {
            HasKey(k => new { k.ProcessId, k.ProjectRole });

            ToTable(tableName: "ProcessMonitoringRoleTbl");

            HasRequired(s => s.Process)
                .WithMany(s => s.MonitoringRoleList)
                .WillCascadeOnDelete(false);
        }
    }
}
