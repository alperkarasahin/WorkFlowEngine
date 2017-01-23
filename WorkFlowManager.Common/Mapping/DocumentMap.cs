using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class DocumentMap : BaseTableMap<Document>
    {
        public DocumentMap()
        {
            ToTable("DocumentTbl");

            //one-to-many
            HasRequired(s => s.Owner) // Belge nin belge islemi
                .WithMany(s => s.DocumentList) // Belge işlemin birden çok belgesi olabilir
                .WillCascadeOnDelete(false);


            Property(s => s.MediaName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_FileMediaName", 1) { IsUnique = true }));


            Property(s => s.FileType)
                .IsRequired();


            Property(s => s.FileName)
                .IsRequired()
                .HasMaxLength(255);

            Property(s => s.MimeType)
                .IsRequired()
                .HasMaxLength(255);

            Property(s => s.Extension)
                .IsRequired()
                .HasMaxLength(10);

            Property(t => t.OwnerId)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_FileOwner", 1) { IsUnique = true }));

            Property(t => t.FileName)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_FileOwner", 2) { IsUnique = true }));

        }
    }
}
