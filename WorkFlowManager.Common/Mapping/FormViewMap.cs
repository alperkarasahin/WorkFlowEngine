using WorkFlowManager.Common.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;

namespace WorkFlowManager.Common.Mapping
{
    public class FormViewMap : BaseTableMap<FormView>
    {
        public FormViewMap()
        {
            ToTable(tableName: "FormViewTbl");

            HasRequired(s => s.Task)
                .WithMany(s => s.FormViewList)
                .WillCascadeOnDelete(false);


            Property(t => t.TaskId)
              .IsRequired()
              .HasColumnAnnotation(
                  IndexAnnotation.AnnotationName,
                  new IndexAnnotation(new[]
                  {
                      new IndexAttribute(name: "IX_FormTanim", order: 1) { IsUnique = true },
                      new IndexAttribute(name: "IX_FormViewName", order: 1) { IsUnique = true },
                  })
              );

            Property(t => t.FormName)
              .IsRequired()
              .HasMaxLength(50)
              .HasColumnAnnotation(
                  IndexAnnotation.AnnotationName,
                  new IndexAnnotation(
                      new IndexAttribute(name: "IX_FormTanim", order: 2) { IsUnique = true }));


            Property(t => t.FormDescription)
              .HasMaxLength(500);

            Property(t => t.ViewName)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasColumnAnnotation(
                      IndexAnnotation.AnnotationName,
                      new IndexAnnotation(
                          new IndexAttribute(name: "IX_FormViewName", order: 2) { IsUnique = true }));


        }
    }
}
