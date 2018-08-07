using System.Data.Entity.ModelConfiguration;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{

    public class TestFormMap : EntityTypeConfiguration<TestForm>
    {
        public TestFormMap()
        {

            ToTable("TestFormTbl");
            HasKey(x => x.OwnerId);

            HasRequired(s => s.Owner)
               .WithRequiredDependent()
               .WillCascadeOnDelete(false);
        }
    }
}
