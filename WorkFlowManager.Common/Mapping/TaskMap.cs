using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class TaskMap : BaseTableMap<Task>
    {
        public TaskMap()
        {
            ToTable(tableName: "TaskTbl");

            //one-to-many
            HasRequired(s => s.WorkFlow) // Gorev bir iş akışına bağlı olmalı
                .WithMany(s => s.TaskList) // İş akışının birden fazla görevi olabilir
                .WillCascadeOnDelete(false);

            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            Property(t => t.MethodServiceName)
                .HasMaxLength(100);

            HasOptional(s => s.TopTask)
                .WithMany(s => s.SubTaskList)
                .HasForeignKey(s => s.TopTaskId)
                .WillCascadeOnDelete(false);

        }
    }
}
