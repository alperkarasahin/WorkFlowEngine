using System.Data.Entity.ModelConfiguration;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class HealthInformationFormMap : EntityTypeConfiguration<HealthInformationForm>
    {
        public HealthInformationFormMap()
        {
            ToTable("HealthInformationFormTbl");

            HasRequired(s => s.Owner)
               .WithMany()
               .WillCascadeOnDelete(false);
        }
    }
}
