namespace WorkFlowManager.Common.Tables
{
    public class BusinessProcess : BaseTable
    {
        public int? OwnerId { get; set; }
        public BusinessProcess Owner { get; set; }
        public string Name { get; set; }

        public int? OwnerSubProcessTraceId { get; set; }
        public WorkFlowTrace OwnerSubProcessTrace { get; set; }
    }
}
