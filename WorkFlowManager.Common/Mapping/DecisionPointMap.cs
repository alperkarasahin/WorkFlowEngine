using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Mapping
{
    public class DecisionPointMap : BaseTableMap<DecisionPoint>
    {
        public DecisionPointMap()
        {
            ToTable(tableName: "DecisionPointTbl");
        }
    }
}
