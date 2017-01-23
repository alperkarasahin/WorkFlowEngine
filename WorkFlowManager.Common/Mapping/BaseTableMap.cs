using WorkFlowManager.Common.Tables;
using System.Data.Entity.ModelConfiguration;

namespace WorkFlowManager.Common.Mapping
{
    public class BaseTableMap<T> : EntityTypeConfiguration<T> where T : BaseTable
    {
        public BaseTableMap()
        {
            // primary key belirtiyoruz.
            HasKey(x => x.Id);

        }
    }
}
