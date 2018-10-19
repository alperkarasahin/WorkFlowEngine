using System.Data.Entity.ModelConfiguration;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class BusinessProcessMap : EntityTypeConfiguration<BusinessProcess>
    {
        public BusinessProcessMap()
        {
            ToTable("BusinessProcessTbl");

            HasOptional(s => s.Owner)
               .WithMany()
               .WillCascadeOnDelete(false);
        }
    }
}
