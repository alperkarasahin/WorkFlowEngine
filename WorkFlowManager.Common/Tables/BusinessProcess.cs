namespace WorkFlowManager.Common.Tables
{
    public class BusinessProcess : BaseTable
    {
        public int? OwnerId { get; set; }
        public BaseTable Owner { get; set; }
        public string Name { get; set; }
    }
}
