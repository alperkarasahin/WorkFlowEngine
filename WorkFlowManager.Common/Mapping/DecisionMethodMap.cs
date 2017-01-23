using WorkFlowManager.Common.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;

namespace WorkFlowManager.Common.Mapping
{
    public class DecisionMethodMap : BaseTableMap<DecisionMethod>
    {
        public DecisionMethodMap()
        {
            ToTable(tableName: "DecisionMethodTbl");


            HasRequired(s => s.Task)
                .WithMany(s => s.DecisionMethodList)
                .WillCascadeOnDelete(false);

            Property(t => t.MethodDescription)
                .HasMaxLength(500);

            Property(t => t.TaskId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(name: "IX_MetotTanim", order: 1) { IsUnique = true }));

            Property(t => t.MethodName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(name: "IX_MetotTanim", order: 2) { IsUnique = true }));


        }
    }
}
