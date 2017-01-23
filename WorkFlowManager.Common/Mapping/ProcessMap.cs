using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class ProcessMap : BaseTableMap<Process>
    {

        public ProcessMap()
        {
            ToTable(tableName: "ProcessTbl");

            HasOptional(p => p.NextProcess)
                .WithMany()
                .HasForeignKey(p => p.NextProcessId);

            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            Property(t => t.MessageForMonitor)
                .HasMaxLength(100);

            Property(t => t.Description)
                .HasMaxLength(500);

            Ignore(t => t.IsStandardForm);

            Property(t => t.ProcessUniqueCode)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(name: "IX_GorevIslemKodu", order: 1) { IsUnique = true }));

            HasRequired(s => s.Task)
                .WithMany(s => s.ProcessList)
                .WillCascadeOnDelete(false);

        }
    }
}
