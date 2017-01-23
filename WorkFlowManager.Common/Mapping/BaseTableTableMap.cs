using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class BaseTableTableMap : BaseTableMap<BaseTable>
    {
        public BaseTableTableMap()
        {

            ToTable("BaseTableTbl");
        }
    }
}
