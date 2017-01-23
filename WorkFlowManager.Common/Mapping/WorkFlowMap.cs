using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class WorkFlowMap : BaseTableMap<WorkFlow>
    {
        public WorkFlowMap()
        {
            ToTable(tableName: "WorkFlowTbl");

            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(name: "IX_IsAkisiTanim", order: 1) { IsUnique = true }));
        }
    }
}
